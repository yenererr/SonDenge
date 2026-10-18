using Jam.Core;
using Jam.Missions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// Ana menü: OYNA / NASIL OYNANIR / AYARLAR / ÇIKIŞ ve küçük atıf paneli.
    /// Arayüz Awake'te kodla kurulur; görsel ve metinler Inspector'dan değiştirilebilir.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [Header("Akış")]
        [SerializeField] private string firstLevelScene = "Level1";
        [Tooltip("Bölüm seçiminde gösterilen sahneler ve başlıklar (sırayla 1..5).")]
        [SerializeField] private string[] levelScenes = { "Level1", "Level2", "Level3", "Level4", "Level5" };
        [SerializeField] private string[] levelTitles = { "DEPREM", "FIRTINA", "YANGIN", "YILDIRIM", "ŞİDDETLİ YAĞMUR" };
        [Header("Görsel")]
        [SerializeField] private Texture2D backdrop;
        [SerializeField] private Font font;
        [SerializeField] private string version = "Game Jam 2026 · v0.3";
        [SerializeField, TextArea(3, 12)] private string credits =
            "Palas Pandıras — 48 saatlik game jam ekibi\n\n" +
            "3D paketler: LowPolyInteriorsFree, Low Poly Tavern Pack, 3Digitalis Interior Props, " +
            "Modular Sewer Props (Ata Khani), Roadside Tales Fences (EmaceArt), StylArts, MIX Furniture, FarmHouse " +
            "— Unity Asset Store Standart EULA.\n\nWilson 3D tavuk: Pjager (CC BY 4.0), yumurta: Casca_Servillius (CC BY 4.0) — ThirdParty/Wilson/CREDITS.md\n\nAyrıntılı liste: docs/ASSET_ATTRIBUTIONS_LEVEL1.md";

        private GameObject howToPanel, settingsPanel, creditsPanel, levelPanel;
        private RectTransform levelList;
        private GameObject openPanel;

        private void Awake()
        {
            GameSettings.Apply();
            UiKit.EnsureEventSystem(transform);
            Build();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            if (GameManager.Instance != null) GameManager.Instance.SetPhase(GamePhase.Menu);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && openPanel != null) ClosePanel();
        }

        private void Play()
        {
            RefreshLevelList();
            Open(levelPanel);
        }

        private void LoadLevel(string scene)
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(string.IsNullOrEmpty(scene) ? firstLevelScene : scene);
        }

        /// <summary>Beş bölüm: kilit, en iyi yıldız ve başlat düğmesi. Kayıt yoksa yalnız Bölüm 1 açık.</summary>
        private void RefreshLevelList()
        {
            if (levelList == null) return;
            foreach (Transform child in levelList) Destroy(child.gameObject);
            for (var i = 0; i < levelScenes.Length; i++)
            {
                var number = i + 1;
                var scene = levelScenes[i];
                var unlocked = LevelProgressSave.IsUnlocked(number);
                var stars = LevelProgressSave.BestStars(number);
                var row = UiKit.Rect(levelList, "Bölüm " + number, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f),
                    Vector2.zero, new Vector2(0f, 70f));
                var caption = "BÖLÜM " + number + " · " + (i < levelTitles.Length ? levelTitles[i] : "") + "     " +
                              (unlocked ? new string('●', stars) + new string('○', 3 - stars) : "KİLİTLİ");
                var button = UiKit.Button(row, caption, 28, unlocked ? UiKit.Paper : new Color(0.7f, 0.66f, 0.6f, 1f), UiKit.Ink,
                    () => LoadLevel(scene));
                button.interactable = unlocked;
                if (font != null) button.GetComponentInChildren<Text>().font = font;
            }
        }

        private void Quit()
        {
            if (SceneLoader.Instance != null) SceneLoader.Instance.Quit();
        }

        private void Open(GameObject panel)
        {
            if (openPanel != null) openPanel.SetActive(false);
            openPanel = panel;
            panel.SetActive(true);
        }

        private void ClosePanel()
        {
            if (openPanel != null) openPanel.SetActive(false);
            openPanel = null;
        }

        private Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var label = UiKit.Label(parent, name, text, size, color, anchor, style);
            if (font != null) label.font = font;
            return label;
        }

        private void Build()
        {
            var canvas = UiKit.CreateCanvas(transform, "MenuCanvas", 100).transform;

            var background = UiKit.Stretch(canvas, "BackgroundGradient");
            if (backdrop != null)
            {
                var raw = background.gameObject.AddComponent<RawImage>();
                raw.texture = backdrop;
                raw.raycastTarget = false;
                var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = backdrop.width / (float)backdrop.height;
            }
            else UiKit.Panel(background, new Color32(70, 92, 104, 255));

            // Sol tarafta okunabilirlik için yumuşak kâğıt şeridi (ekranın %40'ından az).
            var shade = UiKit.Rect(canvas, "OkumaGölgesi", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(720f, 0f));
            UiKit.Panel(shade, new Color(0.15f, 0.10f, 0.07f, 0.55f));

            var title = UiKit.Rect(canvas, "TitleGroup", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(110f, -110f), new Vector2(620f, 190f));
            var titleText = Label(title, "Başlık", "PALAS PANDIRAS", 104, UiKit.Cream, TextAnchor.UpperLeft, FontStyle.Bold);
            titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
            UiKit.Border(titleText, new Color(0.25f, 0.14f, 0.08f, 0.9f), 4f);
            var subtitle = Label(title, "AltBaşlık", "Afet gelmeden önce dengeni kur.", 34, UiKit.Mustard,
                TextAnchor.LowerLeft, FontStyle.Italic);
            subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            subtitle.rectTransform.offsetMin = new Vector2(4f, 0f);

            var buttons = UiKit.Rect(canvas, "MenuButtons", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(110f, -110f), new Vector2(460f, 380f));
            var layout = buttons.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 22f;
            layout.childForceExpandHeight = false;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            AddMenuButton(buttons, "PlayButton", "OYNA", UiKit.Orange, Play);
            AddMenuButton(buttons, "HowToButton", "NASIL OYNANIR", UiKit.Paper, () => Open(howToPanel));
            AddMenuButton(buttons, "SettingsButton", "AYARLAR", UiKit.Paper, () => Open(settingsPanel));
            AddMenuButton(buttons, "QuitButton", "ÇIKIŞ", UiKit.Paper, Quit);

            var footer = UiKit.Rect(canvas, "Footer", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(110f, 36f), new Vector2(900f, 40f));
            Label(footer, "Sürüm", version, 22, new Color(1f, 1f, 1f, 0.75f));
            var creditsButton = UiKit.Rect(canvas, "CreditsButton", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-40f, 30f), new Vector2(300f, 54f));
            UiKit.Button(creditsButton, "Emeği geçenler / Atıflar", 22, UiKit.Paper, UiKit.Ink, () => Open(creditsPanel));

            howToPanel = BuildModal(canvas, "HowToPanel", "NASIL OYNANIR",
                "• Her bölümde Wilson önce görevleri anlatır. GÖREVLERE BAŞLA deyince hazırlık süresi akar.\n" +
                "• A/D yürü, W/S merdiven. E ile küçük eşyayı al/bırak veya yakındaki görev noktasını kullan.\n" +
                "• Büyük eşyayı fareyle sürükle; seçip F ile duvara sabitle. Sabit eşya afette kaymaz.\n" +
                "• Ev sivri bir kayanın tepesinde durur: ağırlık bir kenara gidince ev o yöne eğilir. " +
                "26°'yi 1 saniye aşarsa devrilir. Üstteki denge göstergesini izle.\n" +
                "• Hazır olunca AFETE HAZIRIM (BOŞLUK). Süre biterse afet eksik görevlerle başlar.\n" +
                "• Afet sonunda yıldızlı rapor: hazırlık, doğru davranış, denge ve eşya koruma.\n" +
                "• Fare tekeri yakınlaştırır/uzaklaştırır, orta fare kaydırır, Home kadrajı sıfırlar. ESC duraklatır.\n\n" +
                "GERÇEK HAYATTA: Evin ağırlıkla dengelenmesi bu oyunun hayal ürünü kuralıdır. Gerçek afette eşya taşıma! " +
                "Önlemleri afetten ÖNCE al: ağır eşyaları sabitle, depremde ÇÖK – KAPAN – TUTUN, yangında güvenle tahliye ol, " +
                "fırtınada camlardan ve elektrikten uzak dur.");
            levelPanel = BuildLevelSelect(canvas);
            settingsPanel = BuildSettings(canvas);
            creditsPanel = BuildModal(canvas, "CreditsPanel", "EMEĞİ GEÇENLER / ATIFLAR", credits);
        }

        private void AddMenuButton(RectTransform parent, string name, string caption, Color fill,
            UnityEngine.Events.UnityAction action)
        {
            var rt = UiKit.Rect(parent, name, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 74f));
            var button = UiKit.Button(rt, caption, 36, fill, UiKit.Ink, action);
            if (font != null) button.GetComponentInChildren<Text>().font = font;
        }

        private RectTransform ModalFrame(Transform canvas, string name, string heading, out RectTransform body)
        {
            var root = UiKit.Stretch(canvas, name);
            UiKit.Panel(root, new Color(0f, 0f, 0f, 0.55f), true);
            var frame = UiKit.Rect(root, "Çerçeve", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(1060f, 760f));
            var frameImage = UiKit.Panel(frame, UiKit.Paper, true);
            UiKit.Border(frameImage, UiKit.Wood, 4f);
            var head = UiKit.Rect(frame, "Başlık", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -28f), new Vector2(-80f, 70f));
            Label(head, "Yazı", heading, 46, UiKit.Wood, TextAnchor.MiddleCenter, FontStyle.Bold);
            body = UiKit.Rect(frame, "İçerik", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 10f), new Vector2(-110f, -260f));
            var close = UiKit.Rect(frame, "KapatButonu", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 34f), new Vector2(320f, 64f));
            UiKit.Button(close, "GERİ (ESC)", 30, UiKit.Orange, UiKit.Ink, ClosePanel);
            root.gameObject.SetActive(false);
            return root;
        }

        private GameObject BuildModal(Transform canvas, string name, string heading, string text)
        {
            var root = ModalFrame(canvas, name, heading, out var body);
            // Uzun metin için kaydırılabilir alan.
            var scroll = body.gameObject.AddComponent<ScrollRect>();
            var viewport = UiKit.Stretch(body, "Görünüm");
            viewport.gameObject.AddComponent<RectMask2D>();
            var label = Label(viewport, "Yazı", text, 30, UiKit.Ink, TextAnchor.UpperLeft);
            label.lineSpacing = 1.15f;
            label.rectTransform.anchorMin = new Vector2(0f, 1f);
            label.rectTransform.anchorMax = new Vector2(1f, 1f);
            label.rectTransform.pivot = new Vector2(0.5f, 1f);
            var textFitter = label.gameObject.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport;
            scroll.content = label.rectTransform;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            return root.gameObject;
        }

        private GameObject BuildLevelSelect(Transform canvas)
        {
            var root = ModalFrame(canvas, "LevelSelectPanel", "BÖLÜM SEÇ", out var body);
            levelList = UiKit.Rect(body, "Bölümler", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 30f),
                new Vector2(0f, -60f));
            var layout = levelList.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            var reset = UiKit.Rect(body, "Sıfırla", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, -40f), new Vector2(360f, 50f));
            UiKit.Button(reset, "İLERLEMEYİ SIFIRLA", 22, UiKit.Paper, UiKit.Ink, () =>
            {
                LevelProgressSave.ResetAll();
                RefreshLevelList();
            });
            return root.gameObject;
        }

        private GameObject BuildSettings(Transform canvas)
        {
            var root = ModalFrame(canvas, "SettingsPanel", "AYARLAR", out var body);
            var layout = body.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 26f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
            Slider master = null, music = null, sfx = null;
            Toggle shake = null;
            master = SettingRow(body, "Ana ses", GameSettings.Master, v => GameSettings.Master = v);
            music = SettingRow(body, "Müzik", GameSettings.Music, v => GameSettings.Music = v);
            sfx = SettingRow(body, "Efektler", GameSettings.Sfx, v => GameSettings.Sfx = v);
            var shakeRow = UiKit.Rect(body, "SarsıntıAzalt", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 46f));
            shake = UiKit.Toggle(shakeRow, "Kamera sarsıntısını azalt", GameSettings.ReduceShake, v => GameSettings.ReduceShake = v);
            var fullRow = UiKit.Rect(body, "TamEkran", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 46f));
            UiKit.Toggle(fullRow, "Tam ekran", Screen.fullScreen, v => Screen.fullScreen = v);
            var resetRow = UiKit.Rect(body, "Varsayılan", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 60f));
            UiKit.Button(resetRow, "VARSAYILANA DÖNDÜR", 26, UiKit.Paper, UiKit.Ink, () =>
            {
                GameSettings.ResetToDefaults();
                master.SetValueWithoutNotify(GameSettings.Master);
                music.SetValueWithoutNotify(GameSettings.Music);
                sfx.SetValueWithoutNotify(GameSettings.Sfx);
                shake.SetIsOnWithoutNotify(GameSettings.ReduceShake);
            });
            return root.gameObject;
        }

        private Slider SettingRow(RectTransform parent, string caption, float value, UnityEngine.Events.UnityAction<float> onChange)
        {
            var row = UiKit.Rect(parent, caption, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 50f));
            var label = UiKit.Rect(row, "Ad", new Vector2(0f, 0f), new Vector2(0.35f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            Label(label, "Yazı", caption, 30, UiKit.Ink);
            var sliderRect = UiKit.Rect(row, "Kaydırıcı", new Vector2(0.38f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, Vector2.zero);
            return UiKit.Slider(sliderRect, value, onChange);
        }
    }
}
