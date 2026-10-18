using System;
using Jam.Core;
using Jam.Missions;
using TMPro;
using UnityEngine;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Yükleme ekranı (referans 04). Çubuk SceneLoader'ın gerçek AsyncOperation ilerlemesini gösterir
    /// (0–90 % yükleme, 100 % etkinleştirme); sahte süreli animasyon yoktur.
    /// </summary>
    public sealed class LoadingView : MenuPage
    {
        [Serializable]
        public struct Tip
        {
            [Tooltip("true: gerçek hayatta güvenlik; false: oyun mekaniği.")]
            public bool realLife;
            [TextArea(2, 3)] public string text;
        }

        [SerializeField] private RectTransform fill;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text percentText;
        [SerializeField] private TMP_Text tipTitle;
        [SerializeField] private TMP_Text tipText;
        [SerializeField] private float tipSeconds = 4f;
        [SerializeField] private float minVisibleSeconds = 0.6f;
        [SerializeField] private Tip[] tips = new Tip[0];

        private float shown, target, nextTip;
        private int tipIndex;

        public void Begin(LevelDefinition def, ChapterInfo info, Action<string> failed)
        {
            shown = target = 0f;
            SetFill(0f);
            statusText.text = def.levelNumber + ". Bölüm · " + def.disasterNameTr + " yükleniyor...";
            tipIndex = tips.Length > 0 ? UnityEngine.Random.Range(0, tips.Length) : 0;
            ShowTip();
            if (SceneLoader.Instance == null)
            {
                failed?.Invoke("Sahne yükleyici bulunamadı.");
                return;
            }
            SceneLoader.Instance.LoadWithProgress(def.sceneName, p =>
            {
                target = p;
                // Etkinleştirmeden hemen önce menü müziği durur; sonraki sahneye taşmaz.
                if (p >= 1f && AudioManager.Instance != null) AudioManager.Instance.StopMusic();
            }, failed, minVisibleSeconds);
        }

        private void ShowTip()
        {
            nextTip = Time.unscaledTime + tipSeconds;
            if (tips.Length == 0) return;
            var tip = tips[tipIndex % tips.Length];
            if (tipTitle != null) tipTitle.text = tip.realLife ? "Gerçek hayatta:" : "İpucu:";
            tipText.text = tip.text;
        }

        private void Update()
        {
            if (!IsOpen) return;
            shown = Mathf.MoveTowards(shown, target, Time.unscaledDeltaTime * 2.5f);
            SetFill(shown);
            if (Time.unscaledTime >= nextTip && tips.Length > 1)
            {
                tipIndex++;
                ShowTip();
            }
        }

        private void SetFill(float v)
        {
            if (fill != null) fill.anchorMax = new Vector2(Mathf.Clamp01(v), 1f);
            if (percentText != null) percentText.text = "%" + Mathf.RoundToInt(v * 100f);
        }
    }
}
