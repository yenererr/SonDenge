using Jam.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// L1_Hud'un modal panelleri: mağaza çekmecesi, duraklatma, nasıl oynanır ve AFET RAPORU.
    /// Ayrı bir Canvas'ta (üstte) durur; HUD kararsa da paneller net kalır.
    /// </summary>
    public sealed partial class L1_Hud
    {
        [Header("Paneller")]
        [SerializeField, TextArea(4, 14)] private string howToText =
            "• Sol fareyle bir eşyayı tut ve yalnız sağa–sola sürükle; karakterin taşıması gerekmez. Sağ fare iptal eder.\n" +
            "• Ev iki katlı: altta oturma odası, üstte mutfak. Karakter A/D ile yürür, merdivende W/S ile kat değiştirir.\n" +
            "• Ev sivri bir kayanın tepesinde durur. Ağır eşya kenara gittikçe ev o yöne eğilir; üstteki su terazisini izle.\n" +
            "• Eşyayı seçip SABİTLE (F) ile duvara bağla (30 altın). Sabit eşya depremde kaymaz.\n" +
            "• MAĞAZA'dan deprem çantası ve yeni eşya al. Çanta olmadan deprem başlatılamaz.\n" +
            "• Hazır olunca AFETE HAZIRIM (Boşluk). 3-2-1 sonra deprem başlar.\n" +
            "• Fare tekeri yakınlaştırır, orta fare kaydırır, Home kadrajı sıfırlar. ESC duraklatır.\n\n" +
            "GERÇEK HAYATTA: Oyundaki eşya taşıma gücü fantastiktir. Gerçek depremde eşya taşımayın; " +
            "ağır eşyaları önceden duvara sabitleyin ve deprem anında ÇÖK – KAPAN – TUTUN.";

        private GameObject modalRoot, shopPanel, pausePanel, howToPanel, reportPanel;
        private RectTransform shopList;
        private Text shopBudget, shopFooter, musicLabel, reportTitle, reportSubtitle, reportBody, reportScore;
        private Image reportFrame;
        private Button nextButton;
        private Text nextLabel;
        private Button[] categoryButtons;
        private int shopCategory;
        private bool shopWasOpen;
        private int shopBudgetSeen = -1;

        // ------------------------------------------------------------------ döngü
        private void UpdatePanels()
        {
            var keyboard = Keyboard.current;
            var esc = keyboard != null && keyboard.escapeKey.wasPressedThisFrame;

            var shopOpen = game.IsMarketOpen && game.IsPlanning && !game.IsPausedMenu;
            shopPanel.SetActive(shopOpen);
            if (shopOpen && (!shopWasOpen || shopBudgetSeen != game.Budget)) RefreshShop();
            shopWasOpen = shopOpen;
            if (shopOpen)
            {
                var warning = game.WarningText;
                shopFooter.text = !string.IsNullOrEmpty(warning) ? warning : "Sabitleme aparatı stoğu: " + game.AnchorStock;
                if (esc) game.UiCloseMarket();
            }

            pausePanel.SetActive(game.IsMenuOpen && !game.IsHowToOpen);
            if (pausePanel.activeSelf) musicLabel.text = game.MusicMuted ? "MÜZİK: KAPALI" : "MÜZİK: AÇIK";

            howToPanel.SetActive(game.IsHowToOpen);
            // Kontrolcü, nasıl oynanır açıkken ESC'yi yok sayar; kapatmayı burada yaparız.
            if (game.IsHowToOpen && esc) game.UiCloseHowTo();

            var finished = game.IsReport || game.IsGameOver;
            if (finished && !reportPanel.activeSelf) FillReport();
            reportPanel.SetActive(finished);

            modalRoot.SetActive(shopOpen || pausePanel.activeSelf || howToPanel.activeSelf || finished);
        }

        private void RefreshShop()
        {
            shopBudgetSeen = game.Budget;
            shopBudget.text = game.Budget + " altın";
            var categories = game.UiCategories;
            for (var i = 0; i < categoryButtons.Length; i++)
            {
                categoryButtons[i].gameObject.SetActive(i < categories.Length);
                if (i < categories.Length)
                    categoryButtons[i].targetGraphic.color = i == shopCategory ? UiKit.Mustard : UiKit.Paper;
            }
            for (var i = shopList.childCount - 1; i >= 0; i--) Destroy(shopList.GetChild(i).gameObject);
            foreach (var entry in game.UiShopEntries(shopCategory)) ShopRow(entry);
        }

        private void FillReport()
        {
            var survived = game.IsReport;
            reportTitle.text = "AFET RAPORU";
            reportSubtitle.text = survived ? "EV AYAKTA KALDI" : "DENGE BOZULDU";
            reportSubtitle.color = survived ? UiKit.Olive : UiKit.Coral;
            reportFrame.GetComponent<Outline>().effectColor = survived ? UiKit.Olive : UiKit.Coral;
            reportBody.text = game.ReportText;
            reportScore.text = "Hazırlık puanı\n" + game.SafetyScore + " / 100";
            nextButton.interactable = game.NextLevelAvailable;
            nextLabel.text = game.NextLevelAvailable ? "SONRAKİ BÖLÜM" : "SONRAKİ (YAKINDA)";
        }

        // ------------------------------------------------------------------ kurulum
        private void BuildPanels()
        {
            var canvas = UiKit.CreateCanvas(transform, "Level1Modals", 110);
            modalRoot = canvas.gameObject;
            var root = canvas.transform;
            BuildShop(root);
            pausePanel = BuildPause(root);
            howToPanel = BuildHowTo(root);
            reportPanel = BuildReport(root);
            modalRoot.SetActive(false);
        }

        private static RectTransform Overlay(Transform root, string name, float alpha)
        {
            var overlay = UiKit.Stretch(root, name);
            // Arka plan tıklamaları yutar: panel açıkken dünyaya tıklanmaz.
            UiKit.Panel(overlay, new Color(0f, 0f, 0f, alpha), true);
            overlay.gameObject.SetActive(false);
            return overlay;
        }

        private static RectTransform Frame(Transform parent, Vector2 size, Color border)
        {
            var frame = UiKit.Rect(parent, "Çerçeve", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var image = UiKit.Panel(frame, UiKit.Paper, true);
            UiKit.Border(image, border, 4f);
            return frame;
        }

        private static Button PanelButton(Transform parent, string name, string caption, Color fill,
            Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var rt = UiKit.Rect(parent, name, anchor, anchor, new Vector2(0.5f, 0.5f), position, size);
            return UiKit.Button(rt, caption, 26, fill, UiKit.Ink, action);
        }

        private void BuildShop(Transform root)
        {
            var overlay = Overlay(root, "ShopDrawer", 0.45f);
            shopPanel = overlay.gameObject;
            var drawer = UiKit.Rect(overlay, "Çekmece", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(440f, 0f));
            var drawerImage = UiKit.Panel(drawer, UiKit.Paper, true);
            UiKit.Border(drawerImage, UiKit.Wood, 4f);

            var head = UiKit.Rect(drawer, "Başlık", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -16f), new Vector2(-32f, 60f));
            UiKit.Label(head, "Yazı", "EV MARKETİ", 38, UiKit.Wood, TextAnchor.MiddleLeft, FontStyle.Bold);
            shopBudget = UiKit.Label(head, "Bütçe", "", 26, UiKit.Ink, TextAnchor.MiddleRight, FontStyle.Bold);
            shopBudget.rectTransform.offsetMax = new Vector2(-70f, 0f);
            var close = UiKit.Rect(head, "Kapat", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(56f, 52f));
            UiKit.Button(close, "X", 28, UiKit.Coral, UiKit.Cream, () => game.UiCloseMarket());

            var tabs = UiKit.Rect(drawer, "Sekmeler", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -88f), new Vector2(-32f, 52f));
            var tabLayout = tabs.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 10f;
            tabLayout.childControlWidth = true;
            tabLayout.childForceExpandWidth = true;
            categoryButtons = new Button[6];
            for (var i = 0; i < categoryButtons.Length; i++)
            {
                var index = i;
                var rt = UiKit.Rect(tabs, "Sekme" + i, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                var name = i < game.UiCategories.Length ? game.UiCategories[i] : "";
                categoryButtons[i] = UiKit.Button(rt, name, 24, UiKit.Paper, UiKit.Ink, () => { shopCategory = index; RefreshShop(); });
            }

            var view = UiKit.Rect(drawer, "Liste", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 6f), new Vector2(-32f, -260f));
            view.gameObject.AddComponent<RectMask2D>();
            // Saydam arka plan, satır aralarında da kaydırmayı yakalar.
            UiKit.Panel(view, new Color(0f, 0f, 0f, 0.04f), true);
            shopList = UiKit.Rect(view, "İçerik", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                Vector2.zero, Vector2.zero);
            var listLayout = shopList.gameObject.AddComponent<VerticalLayoutGroup>();
            listLayout.spacing = 10f;
            listLayout.padding = new RectOffset(4, 4, 4, 4);
            listLayout.childControlHeight = false;
            listLayout.childForceExpandHeight = false;
            listLayout.childControlWidth = true;
            shopList.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.content = shopList;
            scroll.viewport = view;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;

            var footer = UiKit.Rect(drawer, "Alt", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 16f), new Vector2(-32f, 64f));
            shopFooter = UiKit.Label(footer, "Yazı", "", 21, UiKit.Ink);
            shopFooter.rectTransform.offsetMax = new Vector2(-150f, 0f);
            var done = UiKit.Rect(footer, "Tamam", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(140f, 56f));
            UiKit.Button(done, "TAMAM", 26, UiKit.Orange, UiKit.Ink, () => game.UiCloseMarket());
        }

        private void ShopRow(L1_ProjectGameController.ShopEntry entry)
        {
            var row = UiKit.Rect(shopList, entry.id, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(0f, 148f));
            var rowImage = UiKit.Panel(row, entry.installed ? new Color32(226, 240, 210, 255) : UiKit.Cream);
            UiKit.Border(rowImage, entry.installed ? UiKit.Olive : new Color(0.38f, 0.24f, 0.15f, 0.6f), 2f);
            var thumbnail = UiKit.Rect(row, "Eşya görseli çerçevesi", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(14f, 0f), new Vector2(96f, 96f));
            var thumbnailFrame = UiKit.Panel(thumbnail, UiKit.Paper);
            UiKit.Border(thumbnailFrame, UiKit.Wood, 1f);
            if (entry.icon != null)
            {
                var picture = UiKit.Rect(thumbnail, "Eşya görseli", Vector2.zero, Vector2.one,
                    new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-12f, -12f));
                var image = picture.gameObject.AddComponent<Image>();
                image.sprite = entry.icon;
                image.preserveAspect = true;
                image.raycastTarget = false;
            }
            var title = UiKit.Label(row, "Ad", entry.title, 23, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            title.rectTransform.offsetMin = new Vector2(122f, 8f);
            title.rectTransform.offsetMax = new Vector2(-112f, -8f);
            var massRect = UiKit.Rect(row, "Kütle alanı", new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(-14f, -8f), new Vector2(94f, 27f));
            UiKit.Label(massRect, "Kütle", entry.mass + " kg", 19, UiKit.Ink, TextAnchor.UpperRight);
            var description = UiKit.Label(row, "Açıklama", entry.description, 19, new Color(0.3f, 0.22f, 0.16f), TextAnchor.MiddleLeft);
            description.rectTransform.offsetMin = new Vector2(122f, 66f);
            description.rectTransform.offsetMax = new Vector2(-14f, -37f);
            if (entry.income > 0)
            {
                var income = UiKit.Label(row, "Bölüm getirisi", "Korunursa +" + entry.income + " altın",
                    19, UiKit.Olive, TextAnchor.MiddleLeft, FontStyle.Bold);
                income.rectTransform.offsetMin = new Vector2(122f, 38f);
                income.rectTransform.offsetMax = new Vector2(-14f, -87f);
            }
            var price = UiKit.Label(row, "Fiyat", entry.cost + " altın", 23, UiKit.Wood, TextAnchor.LowerLeft, FontStyle.Bold);
            price.rectTransform.offsetMin = new Vector2(122f, 8f);
            price.rectTransform.offsetMax = new Vector2(-190f, -113f);
            var buttonRect = UiKit.Rect(row, "Al", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-12f, 8f), new Vector2(170f, 44f));
            var caption = entry.locked ? entry.unlockLevel + ". BÖLÜMDE" : entry.installed ? "KURULDU ✓" :
                !entry.affordable ? "YETERSİZ ALTIN" : "SATIN AL";
            var id = entry.id;
            var button = UiKit.Button(buttonRect, caption, 20, entry.affordable && !entry.locked && !entry.installed
                ? UiKit.Orange : UiKit.Paper, UiKit.Ink, () =>
                {
                    game.UiPurchase(id);
                    shopBudgetSeen = -1; // satın alma sonrası listeyi tazele
                });
            button.interactable = entry.affordable && !entry.locked && !entry.installed;
        }

        private GameObject BuildPause(Transform root)
        {
            var overlay = Overlay(root, "PausePanel", 0.55f);
            var frame = Frame(overlay, new Vector2(560f, 600f), UiKit.Wood);
            var title = UiKit.Rect(frame, "Başlık", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(480f, 70f));
            UiKit.Label(title, "Yazı", "DURAKLATILDI", 44, UiKit.Wood, TextAnchor.MiddleCenter, FontStyle.Bold);
            var size = new Vector2(420f, 66f);
            var top = new Vector2(0.5f, 1f);
            PanelButton(frame, "Devam", "DEVAM ET", UiKit.Orange, top, new Vector2(0f, -150f), size, () => game.UiTogglePause());
            var music = PanelButton(frame, "Müzik", "MÜZİK: AÇIK", UiKit.Paper, top, new Vector2(0f, -232f), size, () => game.UiToggleMusic());
            musicLabel = music.GetComponentInChildren<Text>();
            PanelButton(frame, "NasılOynanır", "NASIL OYNANIR", UiKit.Paper, top, new Vector2(0f, -314f), size, () => game.UiOpenHowTo());
            PanelButton(frame, "YenidenBaşlat", "YENİDEN BAŞLAT", UiKit.Paper, top, new Vector2(0f, -396f), size, () => game.UiRestart());
            PanelButton(frame, "AnaMenü", "ANA MENÜ", UiKit.Coral, top, new Vector2(0f, -478f), size, () => game.UiMainMenu());
            return overlay.gameObject;
        }

        private GameObject BuildHowTo(Transform root)
        {
            var overlay = Overlay(root, "HowToPanel", 0.6f);
            var frame = Frame(overlay, new Vector2(1060f, 760f), UiKit.Wood);
            var title = UiKit.Rect(frame, "Başlık", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(-80f, 70f));
            UiKit.Label(title, "Yazı", "NASIL OYNANIR", 46, UiKit.Wood, TextAnchor.MiddleCenter, FontStyle.Bold);
            var body = UiKit.Rect(frame, "İçerik", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0f, 10f),
                new Vector2(-110f, -250f));
            body.gameObject.AddComponent<RectMask2D>();
            var text = UiKit.Label(body, "Yazı", howToText, 29, UiKit.Ink, TextAnchor.UpperLeft);
            text.lineSpacing = 1.15f;
            text.rectTransform.anchorMin = new Vector2(0f, 1f);
            text.rectTransform.pivot = new Vector2(0.5f, 1f);
            text.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = body.gameObject.AddComponent<ScrollRect>();
            scroll.content = text.rectTransform;
            scroll.viewport = body;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30f;
            PanelButton(frame, "Anladım", "ANLADIM (ESC)", UiKit.Orange, new Vector2(0.5f, 0f), new Vector2(0f, 66f),
                new Vector2(360f, 66f), () => game.UiCloseHowTo());
            return overlay.gameObject;
        }

        private GameObject BuildReport(Transform root)
        {
            var overlay = Overlay(root, "ResultPanel", 0.6f);
            var frame = Frame(overlay, new Vector2(1180f, 800f), UiKit.Olive);
            reportFrame = frame.GetComponent<Image>();
            var title = UiKit.Rect(frame, "Başlık", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -20f), new Vector2(-80f, 64f));
            reportTitle = UiKit.Label(title, "Yazı", "AFET RAPORU", 50, UiKit.Wood, TextAnchor.MiddleCenter, FontStyle.Bold);
            var subtitle = UiKit.Rect(frame, "AltBaşlık", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -86f), new Vector2(-80f, 44f));
            reportSubtitle = UiKit.Label(subtitle, "Yazı", "", 32, UiKit.Olive, TextAnchor.MiddleCenter, FontStyle.Bold);

            var bodyRect = UiKit.Rect(frame, "Ayrıntılar", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                new Vector2(-130f, 20f), new Vector2(-340f, -290f));
            var bodyImage = UiKit.Panel(bodyRect, UiKit.Cream);
            UiKit.Border(bodyImage, new Color(0.38f, 0.24f, 0.15f, 0.6f), 2f);
            reportBody = UiKit.Label(bodyRect, "Yazı", "", 26, UiKit.Ink, TextAnchor.UpperLeft);
            reportBody.lineSpacing = 1.2f;
            reportBody.rectTransform.offsetMin = new Vector2(24f, 18f);
            reportBody.rectTransform.offsetMax = new Vector2(-24f, -18f);

            var scoreRect = UiKit.Rect(frame, "Puan", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-40f, 20f), new Vector2(220f, 220f));
            var scoreImage = UiKit.Panel(scoreRect, UiKit.Mustard);
            UiKit.Border(scoreImage, UiKit.Wood, 3f);
            reportScore = UiKit.Label(scoreRect, "Yazı", "", 34, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);

            var bottom = new Vector2(0.5f, 0f);
            var size = new Vector2(330f, 70f);
            PanelButton(frame, "TekrarDene", "TEKRAR DENE", UiKit.Paper, bottom, new Vector2(-360f, 70f), size, () => game.UiRestart());
            nextButton = PanelButton(frame, "SonrakiBölüm", "SONRAKİ BÖLÜM", UiKit.Orange, bottom, new Vector2(0f, 70f), size,
                () => game.UiNextLevel());
            nextLabel = nextButton.GetComponentInChildren<Text>();
            PanelButton(frame, "AnaMenü", "ANA MENÜ", UiKit.Coral, bottom, new Vector2(360f, 70f), size, () => game.UiMainMenu());
            return overlay.gameObject;
        }
    }
}
