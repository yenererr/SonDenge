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
        private RectTransform hudRoot;
        private Text levelLabel, radioLabel;
        private Image levelIcon, selectedIcon;
        private int shownLevel;
        private RectTransform gaugeMarker, gaugeTrack;
        private Image gaugeMarkerImage;
        private Text gaugeText, budgetText, timerText, warningText, selectedText, selectedDetail,
            selectedDescription, selectedStatus, centerText, safetyText;
        private Button anchorButton, shopButton, readyButton, sellButton;
        private Text anchorLabel;
        private GameObject selectedPanel, centerPanel, safetyPanel, warningPanel, confirmPanel;
        private Text confirmText;
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
            if (game != null) game.ForcedDisasterRequested += ForceCountdown;
        }

        private void OnDisable()
        {
            if (game != null) game.DisasterRequested -= RequestDisaster;
            if (game != null) game.ForcedDisasterRequested -= ForceCountdown;
            if (game != null) game.CountdownActive = false;
        }

        // ------------------------------------------------------------------ eylemler
        private void RequestDisaster()
        {
            if (game == null || !game.IsPlanning || countdownLeft >= 0f) return;
            if (!game.UiCanStartDisaster()) return;
            // Görev modunda eksik görevle başlatma engellenmez, yalnız onay istenir (şartname §4.4).
            if (game.missionMode && game.TasksDone < game.TasksTotal && !confirmPanel.activeSelf)
            {
                confirmText.text = game.TasksDone + "/" + game.TasksTotal + " görev tamamlandı.\nEksik görevler afette riski artırır. Yine de başlatılsın mı?";
                confirmPanel.SetActive(true);
                return;
            }
            if (Mathf.Abs(game.TiltDegrees) > riskWarningAngle * 0.75f && !confirmPanel.activeSelf)
            {
                confirmText.text = "Ev şu an belirgin şekilde eğik. Afet bu eğimi büyütebilir.\nYine de başlatılsın mı?";
                confirmPanel.SetActive(true);
                return;
            }
            BeginCountdown();
        }

        /// <summary>Hazırlık süresi bitti: onay sormadan 3-2-1 ile afet başlar.</summary>
        private void ForceCountdown()
        {
            if (game == null || !game.IsPlanning || countdownLeft >= 0f) return;
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
                levelLabel.text = game.missionMode
                    ? "BÖLÜM " + shownLevel.ToString("00") + " · " + game.DisasterName
                    : "SEVİYE " + shownLevel.ToString("00") + " · " + game.LevelTitle;
                radioLabel.text = game.RadioLine;
                levelIcon.sprite = game.UiLevelIcon;
                levelIcon.enabled = levelIcon.sprite != null;
            }
            UpdatePanels();
            UpdateGauge();
            budgetText.text = game.missionMode ? "Görev " + game.TasksDone + "/" + game.TasksTotal : game.Budget + " altın";
            if (game.IsDisaster) timerText.text = game.DisasterName + "!  " + Mathf.CeilToInt(game.TimeLeft) + " sn";
            else if (game.IsPlanning) timerText.text = "Hazırlık  " + Mathf.CeilToInt(game.TimeLeft) + " sn";
            else timerText.text = "";

            var warning = game.WarningText;
            warningPanel.SetActive(!string.IsNullOrEmpty(warning));
            if (warningPanel.activeSelf) warningText.text = warning;

            var item = game.SelectedItem;
            var planning = game.IsPlanning && countdownLeft < 0f;
            selectedPanel.SetActive(item != null && item.gameObject.activeInHierarchy);
            if (selectedPanel.activeSelf)
            {
                selectedText.text = "Seçili: " + L1_ProjectGameController.PlayerFacingName(item);
                selectedDetail.text = "Ağırlık: " + Mathf.RoundToInt(item.Mass) + " kg";
                selectedDescription.text = game.UiSelectedDescription;
                selectedStatus.text = Mathf.RoundToInt(item.Mass) + " kg     |     " +
                    (item.IsAnchored ? "Sabitli" : "Sabit değil");
                selectedIcon.sprite = game.UiSelectedIcon;
                selectedIcon.enabled = selectedIcon.sprite != null;
            }
            else selectedStatus.text = "Eşya seçilmedi";
            anchorButton.gameObject.SetActive(planning && item != null && item.gameObject.activeInHierarchy);
            anchorButton.interactable = planning && item != null;
            sellButton.interactable = planning && item != null;
            anchorLabel.text = item != null && item.IsAnchored ? "SABİTİ SÖK" :
                game.missionMode ? "SABİTLE (F)" : "SABİTLE (" + game.AnchorCost + ")";
            shopButton.interactable = planning;
            shopButton.gameObject.SetActive(!game.missionMode);
            sellButton.gameObject.SetActive(!game.missionMode);
            readyButton.interactable = planning;

            UpdateCountdown();

            if (game.IsDisaster && !wasDisaster) disasterBannerUntil = Time.unscaledTime + 1.6f;
            wasDisaster = game.IsDisaster;
            safetyPanel.SetActive(game.IsDisaster);
            if (game.IsDisaster)
                safetyText.text = game.missionMode && game.Disaster != Jam.Missions.LevelDisaster.Earthquake
                    ? game.DisasterSafetyLine
                    : game.ResidentHiding
                        ? "SAKLANDIN ●  Çök – Kapan – Tutun.  Gerçek depremde eşya taşımayın."
                        : "BOŞLUK'a bas: Çök – Kapan – Tutun!  Gerçek depremde eşya taşımayın.";
            if (countdownLeft < 0f)
            {
                var showBanner = Time.unscaledTime < disasterBannerUntil;
                centerPanel.SetActive(showBanner);
                if (showBanner) centerText.text = game.DisasterName + "!";
            }
        }

        private void LateUpdate()
        {
            if (anchorButton == null || !anchorButton.gameObject.activeSelf || hudRoot == null || game == null) return;
            var item = game.SelectedItem;
            if (item == null) { anchorButton.gameObject.SetActive(false); return; }
            var camera = cameraController != null ? cameraController.GetComponent<Camera>() : null;
            if (camera == null) camera = Camera.main;
            if (camera == null) return;
            var collider = item.GetComponent<Collider>();
            var point = collider != null ? collider.bounds.center + Vector3.up * (collider.bounds.extents.y + 0.13f) :
                item.transform.position + Vector3.up;
            var screen = camera.WorldToScreenPoint(point);
            if (screen.z <= 0f) { anchorButton.gameObject.SetActive(false); return; }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(hudRoot, screen, null, out var local))
            {
                var rect = hudRoot.rect;
                var button = (RectTransform)anchorButton.transform;
                button.anchoredPosition = new Vector2(Mathf.Clamp(local.x, rect.xMin + 95f, rect.xMax - 95f),
                    Mathf.Clamp(local.y, rect.yMin + 116f, rect.yMax - 65f));
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
            hudRoot = (RectTransform)root;

            // Sol üst: seviye ve radyo.
            var top = UiKit.Rect(root, "TopHud", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, new Vector2(0f, 120f));
            var radio = UiKit.Rect(top, "RadioAlert", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -16f), new Vector2(550f, 110f));
            PaperPanel(radio);
            var iconFrame = UiKit.Rect(radio, "Afet simgesi", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(90f, 80f));
            levelIcon = iconFrame.gameObject.AddComponent<Image>();
            levelIcon.preserveAspect = true;
            levelIcon.raycastTarget = false;
            levelIcon.color = UiKit.Coral;
            var titleArea = UiKit.Rect(radio, "Seviye alanı", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            levelLabel = UiKit.Label(titleArea, "Seviye", levelTitle, 31, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            levelLabel.rectTransform.offsetMin = new Vector2(116f, 52f);
            levelLabel.rectTransform.offsetMax = new Vector2(-17f, -13f);
            radioLabel = UiKit.Label(titleArea, "Radyo", radioLine, 19, UiKit.Ink, TextAnchor.LowerLeft, FontStyle.Italic);
            radioLabel.rectTransform.offsetMin = new Vector2(116f, 13f);
            radioLabel.rectTransform.offsetMax = new Vector2(-17f, -56f);

            // Üst orta: kompakt denge göstergesi (~260 px).
            // Su terazisi: ekranın üst ortasında, evi kapatmayacak ama net okunacak boyutta.
            const float trackWidth = 410f;
            var gauge = UiKit.Rect(top, "BalanceGauge", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(510f, 126f));
            PaperPanel(gauge);
            var gaugeTitle = UiKit.Label(gauge, "Başlık", "EV DENGESİ", 28, UiKit.Ink, TextAnchor.UpperCenter, FontStyle.Bold);
            Pad(gaugeTitle, 8f, 9f);
            gaugeTrack = UiKit.Rect(gauge, "Çubuk", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -45f), new Vector2(trackWidth, 27f));
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
            gaugeText = UiKit.Label(gauge, "Durum", "GÜVENLİ  +0.0°", 25, UiKit.Ink, TextAnchor.LowerCenter, FontStyle.Bold);
            Pad(gaugeText, 8f, 12f);

            // Sağ üst: bütçe ve süre.
            var economy = UiKit.Rect(top, "EconomyAndTimer", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -18f), new Vector2(330f, 92f));
            PaperPanel(economy);
            var economyCoin = UiKit.Rect(economy, "Altın simgesi", new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(0f, 1f), new Vector2(18f, -13f), new Vector2(34f, 34f));
            var economyCoinImage = economyCoin.gameObject.AddComponent<Image>();
            economyCoinImage.sprite = L1_HudSkin.Coin;
            economyCoinImage.raycastTarget = false;
            // Görev modunda altın yok: simge gösterilmez (oyun Awake'te kurulduğu için kontrolcüden okunur).
            if (game != null && game.missionMode) economyCoin.gameObject.SetActive(false);
            budgetText = UiKit.Label(economy, "Bütçe", "750 altın", 28, UiKit.Ink, TextAnchor.UpperRight, FontStyle.Bold);
            Pad(budgetText, 55f, 10f);
            timerText = UiKit.Label(economy, "Süre", "Hazırlık 45 sn", 24, UiKit.Ink, TextAnchor.LowerRight);
            Pad(timerText, 18f, 10f);

            // Uyarı satırı.
            var warn = UiKit.Rect(root, "Uyarı", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -144f), new Vector2(760f, 50f));
            var warnImage = UiKit.Panel(warn, UiKit.Paper);
            L1_HudSkin.Apply(warnImage, L1_HudSkin.Surface.FramedPaper);
            warningText = UiKit.Label(warn, "Yazı", "", 23, UiKit.Ink, TextAnchor.MiddleCenter);
            warningPanel = warn.gameObject;

            // Alt sol: seçili eşya.
            var selected = UiKit.Rect(root, "FurnitureTooltip", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
                new Vector2(24f, 116f), new Vector2(440f, 158f));
            PaperPanel(selected);
            var selectedPicture = UiKit.Rect(selected, "Eşya görseli", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(18f, 0f), new Vector2(110f, 110f));
            L1_HudSkin.Apply(UiKit.Panel(selectedPicture, UiKit.Paper), L1_HudSkin.Surface.Paper);
            var selectedPictureInner = UiKit.Rect(selectedPicture, "Görsel", Vector2.zero, Vector2.one,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-13f, -13f));
            selectedIcon = selectedPictureInner.gameObject.AddComponent<Image>();
            selectedIcon.preserveAspect = true;
            selectedIcon.raycastTarget = false;
            selectedText = UiKit.Label(selected, "Ad", "", 23, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            selectedText.rectTransform.offsetMin = new Vector2(142f, 94f);
            selectedText.rectTransform.offsetMax = new Vector2(-18f, -16f);
            selectedDetail = UiKit.Label(selected, "Ağırlık", "", 21, UiKit.Ink, TextAnchor.MiddleLeft);
            selectedDetail.rectTransform.offsetMin = new Vector2(142f, 58f);
            selectedDetail.rectTransform.offsetMax = new Vector2(-18f, -54f);
            selectedDescription = UiKit.Label(selected, "Açıklama", "", 18, UiKit.Ink, TextAnchor.LowerLeft);
            selectedDescription.rectTransform.offsetMin = new Vector2(142f, 17f);
            selectedDescription.rectTransform.offsetMax = new Vector2(-18f, -88f);
            selectedPanel = selected.gameObject;

            // Alt orta: eylemler.
            var bar = UiKit.Rect(root, "BottomBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 0f), new Vector2(1920f, 108f));
            L1_HudSkin.Apply(UiKit.Panel(bar, UiKit.Wood), L1_HudSkin.Surface.Wood);
            var selectedStatusArea = UiKit.Rect(bar, "Eşya durumu", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(455f, 76f));
            L1_HudSkin.Apply(UiKit.Panel(selectedStatusArea, UiKit.Wood), L1_HudSkin.Surface.DarkButton);
            selectedStatus = UiKit.Label(selectedStatusArea, "Yazı", "Eşya seçilmedi", 23, UiKit.Cream,
                TextAnchor.MiddleCenter, FontStyle.Bold);
            var actions = UiKit.Rect(bar, "Eylemler", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(-90f, 0f), new Vector2(900f, 76f));
            var layout = actions.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 13f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            sellButton = BarButton(actions, "SatButonu", "SAT", UiKit.Paper, () => game.UiSellSelected());
            shopButton = BarButton(actions, "MağazaButonu", "MAĞAZA", UiKit.Paper, () => game.UiToggleMarket());
            readyButton = BarButton(actions, "AfeteHazırımButonu", "AFETE HAZIRIM", UiKit.Orange, RequestDisaster);

            // Seçili eşyanın tepesinde, eşya hareket ettikçe ekran konumunu izleyen eylem.
            var anchorRect = UiKit.Rect(root, "Eşyanın Üstünde Sabitle", new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(190f, 50f));
            anchorButton = UiKit.Button(anchorRect, "SABİTLE (30)", 22, UiKit.Paper, UiKit.Ink,
                () => game.UiToggleAnchor());
            L1_HudSkin.Button(anchorButton, L1_HudSkin.Surface.PaperButton);
            anchorLabel = anchorButton.GetComponentInChildren<Text>();
            anchorButton.gameObject.SetActive(false);

            // Alt sağ: kamera.
            var zoom = UiKit.Rect(root, "ZoomControls", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, 19f), new Vector2(275f, 70f));
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
                new Vector2(-90f, 115f), new Vector2(900f, 30f));
            UiKit.Label(hint, "Yazı", "Sol fare: eşyayı sağa–sola sürükle  ·  Sağ fare: iptal  ·  F: sabitle  ·  A/D: yürü  ·  W/S: merdiven  ·  Tekerlek: yakınlaş",
                19, UiKit.Cream, TextAnchor.MiddleCenter).gameObject.AddComponent<Shadow>();
            var pause = UiKit.Rect(root, "Duraklat", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -122f), new Vector2(64f, 56f));
            L1_HudSkin.Button(UiKit.Button(pause, "II", 28, UiKit.Paper, UiKit.Ink,
                () => game.UiTogglePause()), L1_HudSkin.Surface.PaperButton);

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
            var safetyImage = UiKit.Panel(safety, UiKit.Paper);
            L1_HudSkin.Apply(safetyImage, L1_HudSkin.Surface.FramedPaper);
            safetyText = UiKit.Label(safety, "Yazı", safetyLine, 24, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            safetyPanel = safety.gameObject;
            safetyPanel.SetActive(false);

            // Eğim riski onayı.
            var confirm = UiKit.Rect(root, "RiskOnayı", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(760f, 260f));
            var confirmImage = UiKit.Panel(confirm, UiKit.Paper, true);
            L1_HudSkin.Apply(confirmImage, L1_HudSkin.Surface.FramedPaper);
            confirmText = UiKit.Label(confirm, "Yazı",
                "Ev şu an belirgin şekilde eğik. Deprem bu eğimi büyütebilir.\nYine de başlatılsın mı?", 28, UiKit.Ink,
                TextAnchor.UpperCenter);
            Pad(confirmText, 30f, 30f);
            var yes = UiKit.Rect(confirm, "Evet", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, 28f), new Vector2(300f, 64f));
            L1_HudSkin.Button(UiKit.Button(yes, "BAŞLAT", 28, UiKit.Orange, UiKit.Ink,
                BeginCountdown), L1_HudSkin.Surface.CopperButton);
            var no = UiKit.Rect(confirm, "Hayır", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f),
                new Vector2(12f, 28f), new Vector2(300f, 64f));
            L1_HudSkin.Button(UiKit.Button(no, "DENGELE", 28, UiKit.Paper, UiKit.Ink,
                () => confirmPanel.SetActive(false)), L1_HudSkin.Surface.PaperButton);
            confirmPanel = confirm.gameObject;
            confirmPanel.SetActive(false);
        }

        private static void PaperPanel(RectTransform rt)
        {
            var image = UiKit.Panel(rt, UiKit.Paper);
            L1_HudSkin.Apply(image, L1_HudSkin.Surface.FramedPaper);
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
            var button = UiKit.Button(rt, caption, 25, fill, UiKit.Ink, action);
            L1_HudSkin.Button(button, fill == UiKit.Orange ? L1_HudSkin.Surface.CopperButton : L1_HudSkin.Surface.PaperButton);
            return button;
        }
    }
}
