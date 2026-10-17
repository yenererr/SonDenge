using Jam.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Level 1 uGUI HUD: kompakt denge göstergesi, bütçe/süre, seçili eşya, alt eylem çubuğu,
    /// kamera düğmeleri, 3-2-1 geri sayımı ve deprem uyarısı. Oyun durumunu yalnız
    /// L1_ProjectGameController'dan okur; kendi durum makinesi yoktur.
    /// </summary>
    public sealed partial class L1_Hud : MonoBehaviour
    {
        [SerializeField] private L1_ProjectGameController game;
        [SerializeField] private L1_CameraController cameraController;
        [Header("Metinler")]
        [SerializeField] private string levelTitle = "SEVİYE 01 · İLK SARSINTI";
        [SerializeField] private string radioLine = "RADYO SİVRİTEPE — Bu gece sarsıntı bekleniyor. Hazırlan!";
        [SerializeField] private string safetyLine = "Gerçek depremde eşya taşımayın. Çök – Kapan – Tutun.";
        [Header("Eşikler (derece)")]
        [SerializeField] private float safeAngle = 7f;
        [SerializeField] private float warningAngle = 16f;
        [SerializeField] private float gaugeRangeDegrees = 30f;
        [Header("Geri sayım")]
        [SerializeField] private float countdownSeconds = 3f;
        [SerializeField] private float riskWarningAngle = 18f;

        private CanvasGroup group;
        private Text levelLabel, radioLabel;
        private int shownLevel;
        private RectTransform gaugeMarker, gaugeTrack;
        private Image gaugeMarkerImage;
        private Text gaugeText, budgetText, timerText, warningText, selectedText, centerText, safetyText;
        private Button anchorButton, shopButton, readyButton, sellButton;
        private Text anchorLabel;
        private GameObject selectedPanel, centerPanel, safetyPanel, warningPanel, confirmPanel;
        private float countdownLeft = -1f;
        private float disasterBannerUntil;
        private bool wasDisaster;

        private void Awake()
        {
            if (game == null) game = GetComponentInParent<L1_ProjectGameController>();
            UiKit.EnsureEventSystem(transform);
            Build();
            BuildPanels();
        }

        private void OnEnable()
        {
            if (game != null) game.DisasterRequested += RequestDisaster;
        }

        private void OnDisable()
        {
            if (game != null) game.DisasterRequested -= RequestDisaster;
            if (game != null) game.CountdownActive = false;
        }

        // ------------------------------------------------------------------ eylemler
        private void RequestDisaster()
        {
            if (game == null || !game.IsPlanning || countdownLeft >= 0f) return;
            if (!game.UiCanStartDisaster()) return;
            if (Mathf.Abs(game.TiltDegrees) > riskWarningAngle * 0.75f && !confirmPanel.activeSelf)
            {
                confirmPanel.SetActive(true);
                return;
            }
            BeginCountdown();
        }

        private void BeginCountdown()
        {
            confirmPanel.SetActive(false);
            countdownLeft = countdownSeconds;
            game.CountdownActive = true;
        }

        // ------------------------------------------------------------------ döngü
        private void Update()
        {
            if (game == null) return;
            var modal = game.HasModal;
            group.blocksRaycasts = !modal;
            group.alpha = modal ? 0.35f : 1f;
            if (cameraController != null) cameraController.inputEnabled = !modal;

            if (game.LevelNumber != shownLevel)
            {
                shownLevel = game.LevelNumber;
                levelLabel.text = "SEVİYE " + shownLevel.ToString("00") + " · " + game.LevelTitle;
                radioLabel.text = game.RadioLine;
            }
            UpdatePanels();
            UpdateGauge();
            budgetText.text = game.Budget + " altın";
            if (game.IsDisaster) timerText.text = "DEPREM!  " + Mathf.CeilToInt(game.TimeLeft) + " sn";
            else if (game.IsPlanning) timerText.text = "Hazırlık  " + Mathf.CeilToInt(game.TimeLeft) + " sn";
            else timerText.text = "";

            var warning = game.WarningText;
            warningPanel.SetActive(!string.IsNullOrEmpty(warning));
            if (warningPanel.activeSelf) warningText.text = warning;

            var item = game.SelectedItem;
            var planning = game.IsPlanning && countdownLeft < 0f;
            selectedPanel.SetActive(item != null && item.gameObject.activeInHierarchy);
            if (selectedPanel.activeSelf)
                selectedText.text = L1_ProjectGameController.PlayerFacingName(item) + "  ·  " + Mathf.RoundToInt(item.Mass) + " kg  ·  " +
                    (item.IsAnchored ? "Sabitli" : "Sabit değil");
            anchorButton.interactable = planning && item != null;
            sellButton.interactable = planning && item != null;
            anchorLabel.text = item != null && item.IsAnchored ? "SABİTLEMEYİ SÖK" : "SABİTLE (" + game.AnchorCost + ")";
            shopButton.interactable = planning;
            readyButton.interactable = planning;

            UpdateCountdown();

            if (game.IsDisaster && !wasDisaster) disasterBannerUntil = Time.unscaledTime + 1.6f;
            wasDisaster = game.IsDisaster;
            safetyPanel.SetActive(game.IsDisaster);
            if (countdownLeft < 0f)
            {
                var showBanner = Time.unscaledTime < disasterBannerUntil;
                centerPanel.SetActive(showBanner);
                if (showBanner) centerText.text = "DEPREM!";
            }
        }

        private void UpdateCountdown()
        {
            if (countdownLeft < 0f) return;
            if (!game.IsPlanning) { countdownLeft = -1f; game.CountdownActive = false; return; }
            if (game.IsPausedMenu) return;
            countdownLeft -= Time.unscaledDeltaTime;
            centerPanel.SetActive(true);
            centerText.text = Mathf.CeilToInt(Mathf.Max(0.01f, countdownLeft)).ToString();
            if (countdownLeft > 0f) return;
            countdownLeft = -1f;
            game.CountdownActive = false;
            game.UiStartDisaster();
        }

        private void UpdateGauge()
        {
            var angle = game.TiltDegrees;
            var abs = Mathf.Abs(angle);
            var half = gaugeTrack.rect.width * 0.5f - 24f;
            gaugeMarker.anchoredPosition = new Vector2(Mathf.Clamp(angle / gaugeRangeDegrees, -1f, 1f) * half, 0f);
            Color color;
            string status;
            if (abs >= game.CollapseAngle) { color = UiKit.Coral; status = "ÇÖKÜYOR"; }
            else if (abs > warningAngle) { color = UiKit.Coral; status = "KRİTİK"; }
            else if (abs > safeAngle) { color = UiKit.Mustard; status = "DENGESİZ"; }
            else { color = UiKit.Teal; status = "GÜVENLİ"; }
            if (abs > warningAngle) color = Color.Lerp(color, Color.white, Mathf.PingPong(Time.unscaledTime * 3f, 0.35f));
            gaugeMarkerImage.color = color;
            var arrow = abs < 0.5f ? "" : angle > 0f ? "  ▶" : "◀  ";
            gaugeText.text = (angle <= -0.5f ? arrow : "") + status + "  " + (angle >= 0f ? "+" : "") +
                angle.ToString("F1") + "°" + (angle >= 0.5f ? arrow : "");
            gaugeText.color = abs > warningAngle ? UiKit.Coral : UiKit.Ink;
        }

        // ------------------------------------------------------------------ kurulum
        private void Build()
        {
            var canvas = UiKit.CreateCanvas(transform, "Level1Canvas", 100);
            group = canvas.gameObject.AddComponent<CanvasGroup>();
            var root = canvas.transform;

            // Sol üst: seviye ve radyo.
            var top = UiKit.Rect(root, "TopHud", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(0f, 120f));
            var radio = UiKit.Rect(top, "RadioAlert", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -18f), new Vector2(620f, 92f));
            PaperPanel(radio);
            levelLabel = UiKit.Label(radio, "Seviye", levelTitle, 30, UiKit.Wood, TextAnchor.UpperLeft, FontStyle.Bold);
            Pad(levelLabel, 18f, 10f);
            radioLabel = UiKit.Label(radio, "Radyo", radioLine, 21, UiKit.Ink, TextAnchor.LowerLeft, FontStyle.Italic);
            Pad(radioLabel, 18f, 10f);

            // Üst orta: kompakt denge göstergesi (~260 px).
            // Su terazisi: ekranın üst ortasında, evi kapatmayacak ama net okunacak boyutta.
            const float trackWidth = 440f;
            var gauge = UiKit.Rect(top, "BalanceGauge", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -14f), new Vector2(500f, 118f));
            PaperPanel(gauge);
            var gaugeTitle = UiKit.Label(gauge, "Başlık", "SU TERAZİSİ", 20, UiKit.Wood, TextAnchor.UpperCenter, FontStyle.Bold);
            Pad(gaugeTitle, 8f, 6f);
            gaugeTrack = UiKit.Rect(gauge, "Çubuk", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -36f), new Vector2(trackWidth, 30f));
            var trackImage = UiKit.Panel(gaugeTrack, new Color32(206, 222, 196, 255));
            UiKit.Border(trackImage, UiKit.Wood, 3f);
            var safeZone = UiKit.Rect(gaugeTrack, "GüvenliBölge", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(trackWidth * safeAngle / gaugeRangeDegrees, 0f));
            UiKit.Panel(safeZone, new Color(UiKit.Teal.r, UiKit.Teal.g, UiKit.Teal.b, 0.45f));
            foreach (var side in new[] { -1f, 1f })
            {
                var warnLine = UiKit.Rect(gaugeTrack, "UyarıÇizgisi", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
                    new Vector2(0.5f, 0.5f), new Vector2(side * trackWidth * 0.5f * warningAngle / gaugeRangeDegrees, 0f),
                    new Vector2(3f, 0f));
                UiKit.Panel(warnLine, UiKit.Coral);
            }
            var center = UiKit.Rect(gaugeTrack, "Merkez", new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(3f, 10f));
            UiKit.Panel(center, UiKit.Wood);
            gaugeMarker = UiKit.Rect(gaugeTrack, "Kabarcık", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(46f, 22f));
            gaugeMarkerImage = UiKit.Panel(gaugeMarker, UiKit.Teal);
            UiKit.Border(gaugeMarkerImage, UiKit.Wood, 2f);
            gaugeText = UiKit.Label(gauge, "Durum", "GÜVENLİ  +0.0°", 24, UiKit.Ink, TextAnchor.LowerCenter, FontStyle.Bold);
            Pad(gaugeText, 8f, 8f);

            // Sağ üst: bütçe ve süre.
            var economy = UiKit.Rect(top, "EconomyAndTimer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -18f), new Vector2(330f, 92f));
            PaperPanel(economy);
            budgetText = UiKit.Label(economy, "Bütçe", "750 altın", 30, UiKit.Wood, TextAnchor.UpperRight, FontStyle.Bold);
            Pad(budgetText, 18f, 10f);
            timerText = UiKit.Label(economy, "Süre", "Hazırlık 45 sn", 24, UiKit.Ink, TextAnchor.LowerRight);
            Pad(timerText, 18f, 10f);

            // Uyarı satırı.
            var warn = UiKit.Rect(root, "Uyarı", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -144f), new Vector2(760f, 50f));
            var warnImage = UiKit.Panel(warn, new Color32(255, 232, 214, 240));
            UiKit.Border(warnImage, UiKit.Coral, 2f);
            warningText = UiKit.Label(warn, "Yazı", "", 23, UiKit.Ink, TextAnchor.MiddleCenter);
            warningPanel = warn.gameObject;

            // Alt sol: seçili eşya.
            var selected = UiKit.Rect(root, "FurnitureTooltip", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 24f), new Vector2(520f, 64f));
            PaperPanel(selected);
            selectedText = UiKit.Label(selected, "Yazı", "", 24, UiKit.Ink);
            Pad(selectedText, 18f, 6f);
            selectedPanel = selected.gameObject;

            // Alt orta: eylemler.
            var bar = UiKit.Rect(root, "BottomBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(70f, 24f), new Vector2(860f, 72f));
            var layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            anchorButton = BarButton(bar, "SabitleButonu", "SABİTLE (30)", UiKit.Paper, () => game.UiToggleAnchor());
            anchorLabel = anchorButton.GetComponentInChildren<Text>();
            sellButton = BarButton(bar, "SatButonu", "SAT", UiKit.Paper, () => game.UiSellSelected());
            shopButton = BarButton(bar, "MağazaButonu", "MAĞAZA", UiKit.Paper, () => game.UiOpenMarket());
            readyButton = BarButton(bar, "AfeteHazırımButonu", "AFETE HAZIRIM", UiKit.Orange, RequestDisaster);

            // Alt sağ: kamera.
            var zoom = UiKit.Rect(root, "ZoomControls", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 24f), new Vector2(300f, 64f));
            var zoomLayout = zoom.gameObject.AddComponent<HorizontalLayoutGroup>();
            zoomLayout.spacing = 10f;
            zoomLayout.childControlWidth = true;
            zoomLayout.childForceExpandWidth = true;
            BarButton(zoom, "Yakınlaş", "+", UiKit.Paper, () => { if (cameraController != null) cameraController.ZoomIn(); });
            BarButton(zoom, "Uzaklaş", "−", UiKit.Paper, () => { if (cameraController != null) cameraController.ZoomOut(); });
            var fit = BarButton(zoom, "Sığdır", "SIĞDIR", UiKit.Paper, () => { if (cameraController != null) cameraController.Fit(); });
            fit.GetComponentInChildren<Text>().fontSize = 20;
            var fitLayout = fit.gameObject.AddComponent<LayoutElement>();
            fitLayout.flexibleWidth = 2f;
            var hint = UiKit.Rect(root, "KontrolIpucu", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(70f, 100f), new Vector2(860f, 30f));
            UiKit.Label(hint, "Yazı", "Sol fare: eşyayı sağa–sola sürükle  ·  Sağ fare: iptal  ·  F: sabitle  ·  A/D: yürü  ·  W/S: merdiven  ·  Tekerlek: yakınlaş",
                19, UiKit.Cream, TextAnchor.MiddleCenter).gameObject.AddComponent<Shadow>();
            var pause = UiKit.Rect(root, "Duraklat", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -122f), new Vector2(64f, 56f));
            UiKit.Button(pause, "II", 28, UiKit.Paper, UiKit.Ink, () => game.UiTogglePause());

            // Orta: geri sayım / DEPREM!
            var centerRect = UiKit.Rect(root, "CountdownOverlay", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(600f, 220f));
            centerText = UiKit.Label(centerRect, "Yazı", "", 150, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Border(centerText, new Color(0.35f, 0.12f, 0.06f, 0.95f), 5f);
            centerPanel = centerRect.gameObject;
            centerPanel.SetActive(false);

            // Afet sırasında gerçek hayat satırı.
            var safety = UiKit.Rect(root, "DisasterNotice", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 110f), new Vector2(820f, 52f));
            var safetyImage = UiKit.Panel(safety, new Color32(255, 240, 220, 235));
            UiKit.Border(safetyImage, UiKit.Coral, 2f);
            safetyText = UiKit.Label(safety, "Yazı", safetyLine, 24, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            safetyPanel = safety.gameObject;
            safetyPanel.SetActive(false);

            // Eğim riski onayı.
            var confirm = UiKit.Rect(root, "RiskOnayı", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(760f, 260f));
            var confirmImage = UiKit.Panel(confirm, UiKit.Paper, true);
            UiKit.Border(confirmImage, UiKit.Coral, 4f);
            var confirmText = UiKit.Label(confirm, "Yazı",
                "Ev şu an belirgin şekilde eğik. Deprem bu eğimi büyütebilir.\nYine de başlatılsın mı?", 28, UiKit.Ink,
                TextAnchor.UpperCenter);
            Pad(confirmText, 30f, 30f);
            var yes = UiKit.Rect(confirm, "Evet", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, 28f), new Vector2(300f, 64f));
            UiKit.Button(yes, "BAŞLAT", 28, UiKit.Orange, UiKit.Ink, BeginCountdown);
            var no = UiKit.Rect(confirm, "Hayır", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f),
                new Vector2(12f, 28f), new Vector2(300f, 64f));
            UiKit.Button(no, "DENGELE", 28, UiKit.Paper, UiKit.Ink, () => confirmPanel.SetActive(false));
            confirmPanel = confirm.gameObject;
            confirmPanel.SetActive(false);
        }

        private static void PaperPanel(RectTransform rt)
        {
            var image = UiKit.Panel(rt, UiKit.Paper);
            UiKit.Border(image, UiKit.Wood, 3f);
        }

        private static void Pad(Text label, float x, float y)
        {
            label.rectTransform.offsetMin = new Vector2(x, y);
            label.rectTransform.offsetMax = new Vector2(-x, -y);
        }

        private static Button BarButton(RectTransform parent, string name, string caption, Color fill,
            UnityEngine.Events.UnityAction action)
        {
            var rt = UiKit.Rect(parent, name, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return UiKit.Button(rt, caption, 26, fill, UiKit.Ink, action);
        }
    }
}
