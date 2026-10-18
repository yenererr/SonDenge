using System;
using System.Collections.Generic;
using Jam.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Ayarlar (referans 06). Kontroller bir taslak üzerinde çalışır; UYGULA kaydeder (PlayerPrefs) ve uygular.
    /// Uygulanmamış değişiklikle GERİ/ESC → "Kaydetmeden çıkılsın mı?". Riskli görüntü değişikliği 10 sn içinde
    /// onaylanmazsa geri alınır. Yalnız gerçekten çalışan ayarlar gösterilir (parlaklık yok).
    /// Menüde MenuFlowController ile, oyun içinde Duraklat menüsünden (Closed geri çağrısı) kullanılır.
    /// </summary>
    public sealed class SettingsView : MenuPage
    {
        [SerializeField] private ConfirmDialog confirm;
        [SerializeField] private TabButtonView[] tabs = new TabButtonView[0];
        [SerializeField] private GameObject[] panels = new GameObject[0];
        [SerializeField] private TMP_Text panelTitle;
        [SerializeField] private string[] panelTitles = { "Ses Ayarları", "Görüntü Ayarları", "Oynanış Ayarları", "Dil", "Erişilebilirlik" };
        [Header("Ses")]
        [SerializeField] private Slider master;
        [SerializeField] private Slider music;
        [SerializeField] private Slider sfx;
        [SerializeField] private TMP_Text masterValue, musicValue, sfxValue;
        [SerializeField] private Toggle mute;
        [Header("Görüntü")]
        [SerializeField] private TMP_Dropdown resolution;
        [SerializeField] private Toggle fullscreen;
        [SerializeField] private Toggle vsync;
        [SerializeField] private TMP_Dropdown quality;
        [Header("Oynanış / Erişilebilirlik")]
        [SerializeField] private Toggle reduceShake;
        [SerializeField] private Button resetProgress;
        [SerializeField] private Toggle reduceMotion;
        [Header("Alt düğmeler")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button defaultsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text statusText;

        /// <summary>Oyun içi kullanım: sayfa kapanınca çağrılır (menüde boş bırakılır, Flow.Back kullanılır).</summary>
        public Action Closed;

        private static int lastCategory = 1;
        private readonly List<Vector2Int> resolutions = new List<Vector2Int>();
        private bool dirty, populating;
        private float statusUntil;

        private static readonly Dictionary<string, string> QualityTr = new Dictionary<string, string>
        {
            { "Very Low", "Çok Düşük" }, { "Low", "Düşük" }, { "Medium", "Orta" }, { "High", "Yüksek" },
            { "Very High", "Çok Yüksek" }, { "Ultra", "Ultra" }, { "Mobile", "Düşük (Mobil)" }, { "PC", "Yüksek (PC)" },
            { "Performant", "Performans" }, { "Balanced", "Dengeli" }, { "High Fidelity", "Yüksek Kalite" }
        };

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++)
            {
                var index = i;
                tabs[i].Button.onClick.AddListener(() => SelectCategory(index));
            }
            master.onValueChanged.AddListener(_ => Changed(true));
            music.onValueChanged.AddListener(_ => Changed(true));
            sfx.onValueChanged.AddListener(_ => Changed(true));
            mute.onValueChanged.AddListener(_ => Changed(true));
            foreach (var t in new[] { fullscreen, vsync, reduceShake, reduceMotion }) t.onValueChanged.AddListener(_ => Changed(false));
            resolution.onValueChanged.AddListener(_ => Changed(false));
            quality.onValueChanged.AddListener(_ => Changed(false));
            applyButton.onClick.AddListener(Apply);
            defaultsButton.onClick.AddListener(AskDefaults);
            backButton.onClick.AddListener(() => { if (!HandleBack()) Close(); });
            resetProgress.onClick.AddListener(AskResetProgress);
#if UNITY_WEBGL && !UNITY_EDITOR
            resolution.transform.parent.gameObject.SetActive(false);
            vsync.transform.parent.gameObject.SetActive(false);
#endif
        }

        protected override void OnShown()
        {
            Populate();
            SelectCategory(lastCategory);
        }

        protected override void OnHidden()
        {
            // Uygulanmamış ses önizlemesi kalıcı ayara döner.
            GameSettings.Apply();
        }

        private void Populate()
        {
            populating = true;
            master.SetValueWithoutNotify(GameSettings.Master);
            music.SetValueWithoutNotify(GameSettings.Music);
            sfx.SetValueWithoutNotify(GameSettings.Sfx);
            mute.SetIsOnWithoutNotify(GameSettings.Muted);
            fullscreen.SetIsOnWithoutNotify(Screen.fullScreen);
            vsync.SetIsOnWithoutNotify(GameSettings.VSync);
            reduceShake.SetIsOnWithoutNotify(GameSettings.ReduceShake);
            reduceMotion.SetIsOnWithoutNotify(GameSettings.ReduceMotion);

            resolutions.Clear();
            foreach (var r in Screen.resolutions)
            {
                var v = new Vector2Int(r.width, r.height);
                if (!resolutions.Contains(v)) resolutions.Add(v);
            }
            resolutions.Sort((a, b) => b.x != a.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));
            var current = new Vector2Int(Screen.width, Screen.height);
            if (!resolutions.Contains(current)) resolutions.Insert(0, current);
            resolution.ClearOptions();
            var labels = new List<string>();
            foreach (var r in resolutions) labels.Add(r.x + " x " + r.y);
            resolution.AddOptions(labels);
            resolution.SetValueWithoutNotify(resolutions.IndexOf(current));

            quality.ClearOptions();
            var names = new List<string>();
            foreach (var n in QualitySettings.names) names.Add(QualityTr.TryGetValue(n, out var tr) ? tr : n);
            quality.AddOptions(names);
            quality.SetValueWithoutNotify(GameSettings.Quality);
            RefreshValues();
            dirty = false;
            populating = false;
            SetStatus("", 0f);
        }

        private void SelectCategory(int index)
        {
            index = Mathf.Clamp(index, 0, panels.Length - 1);
            lastCategory = index;
            for (var i = 0; i < panels.Length; i++) panels[i].SetActive(i == index);
            for (var i = 0; i < tabs.Length; i++) tabs[i].SetActive(i == index);
            if (panelTitle != null && index < panelTitles.Length) panelTitle.text = panelTitles[index];
        }

        private void Changed(bool audioPreview)
        {
            if (populating) return;
            dirty = true;
            RefreshValues();
            if (audioPreview) AudioListener.volume = mute.isOn ? 0f : master.value;
            SetStatus("Değişiklikleri kaydetmek için UYGULA'ya bas.", 0f);
        }

        private void RefreshValues()
        {
            masterValue.text = Mathf.RoundToInt(master.value * 100f).ToString();
            musicValue.text = Mathf.RoundToInt(music.value * 100f).ToString();
            sfxValue.text = Mathf.RoundToInt(sfx.value * 100f).ToString();
        }

        private void Apply()
        {
            GameSettings.Master = master.value;
            GameSettings.Music = music.value;
            GameSettings.Sfx = sfx.value;
            GameSettings.Muted = mute.isOn;
            GameSettings.ReduceShake = reduceShake.isOn;
            GameSettings.ReduceMotion = reduceMotion.isOn;
            GameSettings.VSync = vsync.isOn;
            GameSettings.Quality = quality.value;
            GameSettings.Save();
            dirty = false;

            var oldRes = new Vector2Int(Screen.width, Screen.height);
            var oldFull = Screen.fullScreen;
            var newRes = resolution.value >= 0 && resolution.value < resolutions.Count ? resolutions[resolution.value] : oldRes;
            var newFull = fullscreen.isOn;
            if (newRes != oldRes || newFull != oldFull)
            {
                Screen.SetResolution(newRes.x, newRes.y, newFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
                confirm.Show("GÖRÜNTÜ AYARI", "Yeni görüntü ayarı korunsun mu?", "KORU", "GERİ AL",
                    () => SetStatus("Ayarlar uygulandı.", 2.5f),
                    () =>
                    {
                        Screen.SetResolution(oldRes.x, oldRes.y, oldFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
                        Populate();
                        SetStatus("Görüntü ayarı geri alındı.", 2.5f);
                    }, 10f);
            }
            else SetStatus("Ayarlar uygulandı.", 2.5f);
            Sound(MenuSound.Confirm);
        }

        private void AskDefaults()
        {
            confirm.Show("VARSAYILANA DÖN", "Bütün ayarlar varsayılan değerlere dönsün mü?\n<size=85%>Bölüm ilerlemen silinmez.</size>",
                "EVET", "VAZGEÇ", () =>
                {
                    GameSettings.ResetToDefaults();
                    Populate();
                    SetStatus("Varsayılan ayarlar yüklendi.", 2.5f);
                });
        }

        private void AskResetProgress()
        {
            confirm.Show("İLERLEMEYİ SIFIRLA", "Bütün bölüm yıldızları ve kilitler sıfırlansın mı?\n<size=85%>Bu işlem geri alınamaz.</size>",
                "SIFIRLA", "VAZGEÇ", () =>
                {
                    LevelProgressSave.ResetAll();
                    if (Flow != null) Flow.SelectedChapter = 0;
                    SetStatus("Bölüm ilerlemesi sıfırlandı.", 2.5f);
                });
        }

        public override bool HandleBack()
        {
            if (!dirty) return false;
            confirm.Show("KAYDEDİLMEDİ", "Uygulanmamış değişiklikler var. Kaydetmeden çıkılsın mı?", "ÇIK", "VAZGEÇ", () =>
            {
                dirty = false;
                GameSettings.Apply();
                Close();
            });
            return true;
        }

        private void Close()
        {
            if (Closed != null) Closed();
            else if (Flow != null) Flow.Back();
        }

        private void Sound(MenuSound s)
        {
            if (Flow != null) Flow.Play(s);
        }

        private void SetStatus(string text, float seconds)
        {
            if (statusText == null) return;
            statusText.text = text;
            statusUntil = seconds > 0f ? Time.unscaledTime + seconds : float.MaxValue;
        }

        private void Update()
        {
            if (statusText != null && Time.unscaledTime > statusUntil) statusText.text = "";
        }
    }
}
