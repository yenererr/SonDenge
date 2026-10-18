using Jam.UI.Menu;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Jam.EditorTools.MenuUiFactory;

namespace Jam.EditorTools
{
    /// <summary>
    /// Menünün ortak UI prefab'larını üretir (Assets/Prefabs/UI/Menu). Sahne bu prefab'ların örneklerini kullanır;
    /// metin/ikon farkları sahnede override olarak tutulur.
    /// </summary>
    public static class MenuPrefabBuilder
    {
        public const string Dir = "Assets/Prefabs/UI/Menu";

        public static GameObject WoodPrimary, WoodSecondary, WoodSmall, TabButton, ParchmentPanel, ChapterCard, StarRating,
            WilsonMessage, WilsonPortraitFrame, TipCard, SettingsSliderRow, SettingsToggleRow, SettingsDropdownRow,
            ConfirmDialogPrefab, LoadingProgress, BrandLogo, TaskRow, HelpRow, TitleSign;

        public static void BuildAll()
        {
            WoodPrimary = Save("UI_WoodButton_Primary", BuildWoodButton("UI_WoodButton_Primary", "wood_plank_gold", Ink, MatInk, 560, 112, 44));
            WoodSecondary = Save("UI_WoodButton_Secondary", BuildWoodButton("UI_WoodButton_Secondary", "wood_plank", Cream, MatOnWood, 560, 100, 40));
            WoodSmall = Save("UI_WoodButton_Small", BuildWoodButton("UI_WoodButton_Small", "wood_plank", Cream, MatOnWood, 340, 84, 32));
            TabButton = Save("UI_WoodTabButton", BuildTab());
            ParchmentPanel = Save("UI_ParchmentPanel", BuildParchmentPanel());
            StarRating = Save("UI_StarRating", BuildStars());
            ChapterCard = Save("UI_ChapterCard", BuildCard());
            WilsonMessage = Save("UI_WilsonMessage", BuildBubble());
            WilsonPortraitFrame = Save("UI_WilsonPortrait", BuildPortrait());
            TipCard = Save("UI_TipCard", BuildTipCard());
            SettingsSliderRow = Save("UI_SettingsRow_Slider", BuildSliderRow());
            SettingsToggleRow = Save("UI_SettingsRow_Toggle", BuildToggleRow());
            SettingsDropdownRow = Save("UI_SettingsRow_Dropdown", BuildDropdownRow());
            ConfirmDialogPrefab = Save("UI_ConfirmDialog", BuildConfirm());
            LoadingProgress = Save("UI_LoadingProgress", BuildProgress());
            BrandLogo = Save("UI_BrandLogo", BuildLogo());
            TitleSign = Save("UI_TitleSign", BuildTitleSign());
            TaskRow = Save("UI_TaskRow", BuildTaskRow());
            HelpRow = Save("UI_HelpRow", BuildHelpRow());
        }

        private static GameObject Save(string name, GameObject root)
        {
            var path = Dir + "/" + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static RectTransform Root(string name, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        // ------------------------------------------------------------------ düğmeler
        private static GameObject BuildWoodButton(string name, string sprite, Color textColor, Material mat, float w, float h, float fontSize)
        {
            var root = Root(name, w, h);
            var glow = Stretch("Glow", root, -46, -46, -46, -46);
            var glowImg = Img(glow, "glow", new Color(1f, 0.85f, 0.4f, 0f));
            var bg = Img(root, sprite, Color.white, true);
            // Image köke eklenince Glow'un önüne geçer; Glow ayrı bir çocuk olduğundan arkada kalması için ilk sırada tutulur.
            var icon = Anchored("Icon", root, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(h * 0.75f, 0f), new Vector2(h * 0.5f, h * 0.5f));
            IconImg(icon, "play", textColor == Cream ? Cream : Ink);
            var label = Label(root, "Label", "DÜĞME", fontSize, textColor, TextAlignmentOptions.Center, true, mat, h * 1.15f, 4, h * 0.5f, 8);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = fontSize * 0.6f;
            label.fontSizeMax = fontSize;
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.97f, 0.9f);
            colors.selectedColor = new Color(1f, 0.97f, 0.9f);
            colors.pressedColor = new Color(0.86f, 0.8f, 0.72f);
            colors.disabledColor = new Color(0.62f, 0.6f, 0.58f, 0.75f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            var fb = root.gameObject.AddComponent<WoodButtonFeedback>();
            Set(fb, "glow", glowImg);
            return root.gameObject;
        }

        private static GameObject BuildTab()
        {
            var root = Root("UI_WoodTabButton", 300, 84);
            var bg = Img(root, "parchment_soft", Color.white, true);
            var icon = Anchored("Icon", root, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(46, 0), new Vector2(46, 46));
            IconImg(icon, "gamepad", Ink);
            var label = Label(root, "Label", "Sekme", 30, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk, 84, 0, 34, 0);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 20;
            label.fontSizeMax = 30;
            var arrow = Anchored("ActiveArrow", root, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-22, 0), new Vector2(22, 22));
            IconImg(arrow, "arrow_right", Ink);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            var fb = root.gameObject.AddComponent<WoodButtonFeedback>();
            SetValue(fb, "hoverScale", p => p.floatValue = 1.03f);
            var view = root.gameObject.AddComponent<TabButtonView>();
            Set(view, "button", button);
            Set(view, "background", bg);
            Set(view, "icon", icon.GetComponent<Image>());
            Set(view, "label", label);
            Set(view, "activeSprite", Sprite("wood_plank_gold"));
            Set(view, "inactiveSprite", Sprite("parchment_soft"));
            Set(view, "activeArrow", arrow.gameObject);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ paneller
        /// <summary>Ahşap çerçeve + içte parşömen (referans 03/05/06/07 büyük panelleri).</summary>
        private static GameObject BuildParchmentPanel()
        {
            var root = Root("UI_ParchmentPanel", 1000, 640);
            var shadow = Stretch("Shadow", root, -30, -20, -30, -46);
            Img(shadow, "soft_shadow", new Color(0f, 0f, 0f, 0.45f));
            var frame = Stretch("Frame", root);
            Img(frame, "wood_panel", Color.white, true);
            var paper = Stretch("Paper", root, 26, 26, 26, 26);
            Img(paper, "parchment", Color.white);
            Stretch("Content", paper);
            return root.gameObject;
        }

        private static GameObject BuildStars()
        {
            var root = Root("UI_StarRating", 230, 70);
            var h = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 8;
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = h.childControlHeight = false;
            for (var i = 0; i < 3; i++)
            {
                var s = Node("Star" + (i + 1), root);
                s.sizeDelta = new Vector2(66, 66);
                var img = s.gameObject.AddComponent<Image>();
                img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Levels/Level1/Art/UI/L1_UI_Star_Inactive.png");
                img.preserveAspect = true;
                img.raycastTarget = false;
            }
            return root.gameObject;
        }

        private static GameObject BuildCard()
        {
            var root = Root("UI_ChapterCard", 314, 572);
            var glow = Stretch("Glow", root, -40, -40, -40, -40);
            var glowImg = Img(glow, "glow", new Color(1f, 0.82f, 0.32f, 0f));
            var hit = Img(root, (Sprite)null, new Color(0, 0, 0, 0), true);
            var visual = Stretch("Visual", root);
            var dim = Group(visual.gameObject);
            var frame = Stretch("Frame", visual);
            Img(frame, "card_frame", Color.white);
            var thumb = Stretch("Thumbnail", visual, 20, 96, 20, 172);
            var raw = thumb.gameObject.AddComponent<RawImage>();
            MenuSceneBuilder.ImportBackground(MenuSceneBuilder.BgDir + "/bg_02_bolum_secimi.png");
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(MenuSceneBuilder.BgDir + "/bg_02_bolum_secimi.png");
            raw.uvRect = new Rect(0.56f, 0.3f, 0.32f, 0.32f * 1672f / 941f * 304f / 274f);
            raw.raycastTarget = false;
            thumb.gameObject.AddComponent<RectMask2D>();
            var overlay = Stretch("Overlay", thumb);
            var overlayImg = Img(overlay, "round_white", new Color(1, 1, 1, 0.2f));
            overlayImg.type = Image.Type.Simple;
            var badge = Anchored("DisasterBadge", thumb, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10, -10), new Vector2(64, 64));
            Img(badge, "round_white", new Color(0.1f, 0.08f, 0.08f, 0.7f));
            var badgeIcon = Stretch("Icon", badge, 12, 12, 12, 12);
            var badgeImg = IconImg(badgeIcon, "mountain", Cream);
            // Başlık şeridi (kartın üst kenarına taşar)
            var ribbon = Anchored("Ribbon", visual, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 14), new Vector2(286, 120));
            var ribbonImg = Img(ribbon, "ribbon", Color.white);
            var number = Label(ribbon, "Number", "1. BÖLÜM", 30, Cream, TextAlignmentOptions.Top, true, MatOnWood, 10, 12, 10, 60);
            var nameText = Label(ribbon, "Name", "DEPREM", 52, Cream, TextAlignmentOptions.Bottom, true, MatTitle, 8, 40, 8, 10);
            nameText.textWrappingMode = TextWrappingModes.NoWrap;
            nameText.enableAutoSizing = true;
            nameText.fontSizeMin = 30;
            nameText.fontSizeMax = 54;
            // Yıldızlar ve süre
            var stars = (GameObject)PrefabUtility.InstantiatePrefab(StarRating, visual);
            var srt = (RectTransform)stars.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0f);
            srt.pivot = new Vector2(0.5f, 0f);
            srt.anchoredPosition = new Vector2(0, 92);
            var timeBar = Anchored("Duration", visual, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 24), new Vector2(262, 58));
            Img(timeBar, "round_white", new Color(0.05f, 0.05f, 0.08f, 0.75f));
            var clock = Anchored("Clock", timeBar, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(40, 0), new Vector2(38, 38));
            IconImg(clock, "clock", Cream);
            var duration = Label(timeBar, "Text", "12 saniye", 30, Cream, TextAlignmentOptions.Midline, true, MatInk, 64, 0, 14, 0);
            // Kilit
            var lockGroup = Stretch("Lock", visual, 20, 96, 20, 20);
            Img(lockGroup, "round_white", new Color(0.04f, 0.04f, 0.06f, 0.62f)).type = Image.Type.Simple;
            var lockIcon = Centered("LockIcon", lockGroup, 0, 40, 110, 110);
            IconImg(lockIcon, "lock", Cream);
            var lockText = Label(lockGroup, "Text", "KİLİTLİ", 34, Cream, TextAlignmentOptions.Center, true, MatOnWood, 0, 250, 0, 120);
            var button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = hit;
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var view = root.gameObject.AddComponent<ChapterCardView>();
            Set(view, "button", button);
            Set(view, "visual", visual);
            Set(view, "ribbon", ribbonImg);
            Set(view, "numberText", number);
            Set(view, "nameText", nameText);
            Set(view, "thumbnail", raw);
            Set(view, "overlay", overlayImg);
            Set(view, "disasterIcon", badgeImg);
            var starImgs = stars.GetComponentsInChildren<Image>();
            SetArray(view, "stars", new Object[] { starImgs[0], starImgs[1], starImgs[2] });
            Set(view, "starOn", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Levels/Level1/Art/UI/L1_UI_Star_Active.png"));
            Set(view, "starOff", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Levels/Level1/Art/UI/L1_UI_Star_Inactive.png"));
            Set(view, "durationText", duration);
            Set(view, "lockGroup", lockGroup.gameObject);
            Set(view, "lockText", lockText);
            Set(view, "glow", glowImg);
            Set(view, "dim", dim);
            var fb = root.gameObject.AddComponent<WoodButtonFeedback>();
            SetValue(fb, "hoverScale", p => p.floatValue = 1.02f);
            return root.gameObject;
        }

        private static GameObject BuildBubble()
        {
            var root = Root("UI_WilsonMessage", 560, 220);
            var tail = Anchored("Tail", root, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(46, 64), new Vector2(84, 84));
            Img(tail, "bubble_tail", Color.white, false).preserveAspect = false;
            var body = Stretch("Body", root);
            Img(body, "bubble", Color.white);
            var text = Label(body, "Text", "Merhaba! Ben Wilson!", 32, Ink, TextAlignmentOptions.MidlineLeft, true, MatInk, 34, 22, 30, 22);
            text.enableAutoSizing = true;
            text.fontSizeMin = 20;
            text.fontSizeMax = 34;
            return root.gameObject;
        }

        private static GameObject BuildPortrait()
        {
            var root = Root("UI_WilsonPortrait", 520, 520);
            var shadow = Anchored("GroundShadow", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), new Vector2(0, 30), new Vector2(300, 60));
            Img(shadow, "soft_shadow", new Color(0, 0, 0, 0.35f)).type = Image.Type.Simple;
            var raw = Stretch("Render", root).gameObject.AddComponent<RawImage>();
            raw.raycastTarget = false;
            return root.gameObject;
        }

        private static GameObject BuildTipCard()
        {
            var root = Root("UI_TipCard", 440, 200);
            Img(root, "parchment", Color.white);
            var bulb = Anchored("Icon", root, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(50, -52), new Vector2(54, 54));
            IconImg(bulb, "bulb", Hex("E3A321"));
            Label(root, "Title", "İpucu:", 30, Ink, TextAlignmentOptions.TopLeft, true, MatInk, 90, 28, 20, 120);
            var text = Label(root, "Text", "", 25, Ink, TextAlignmentOptions.TopLeft, false, MatInk, 90, 66, 26, 20);
            text.enableAutoSizing = true;
            text.fontSizeMin = 18;
            text.fontSizeMax = 25;
            return root.gameObject;
        }

        // ------------------------------------------------------------------ ayar satırları
        private static RectTransform Row(string name, string caption, out TextMeshProUGUI label)
        {
            var root = Root(name, 840, 76);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 76;
            label = Label(root, "Label", caption, 32, Ink, TextAlignmentOptions.MidlineLeft, true, MatInk, 12, 0, 470, 0);
            var line = Anchored("Line", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(840, 2));
            Img(line, (Sprite)null, new Color(0.55f, 0.42f, 0.3f, 0.35f));
            return root;
        }

        private static GameObject BuildSliderRow()
        {
            var root = Row("UI_SettingsRow_Slider", "Ana Ses", out _);
            var area = Anchored("Slider", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-96, 0), new Vector2(340, 40));
            var track = Stretch("Track", area, 0, 12, 0, 12);
            Img(track, "pill", Hex("D9C6A5"));
            var fillArea = Stretch("Fill Area", area, 10, 12, 10, 12);
            var fill = Node("Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.sizeDelta = new Vector2(10, 0);
            Img(fill, "pill", Hex("2F7FC1"));
            var handleArea = Stretch("Handle Slide Area", area, 18, 0, 18, 0);
            var handle = Node("Handle", handleArea);
            handle.sizeDelta = new Vector2(40, 0);
            handle.anchorMin = new Vector2(0, 0);
            handle.anchorMax = new Vector2(0, 1);
            var handleImg = Img(handle, "knob", Color.white, true, false);
            var slider = area.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 0.8f;
            var value = Anchored("Value", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6, 0), new Vector2(76, 60));
            Text(value, "80", 30, Ink, TextAlignmentOptions.MidlineRight, true, MatInk);
            return root.gameObject;
        }

        private static GameObject BuildToggleRow()
        {
            var root = Row("UI_SettingsRow_Toggle", "Tam Ekran", out _);
            var area = Anchored("Toggle", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-150, 0), new Vector2(290, 56));
            var track = Anchored("Track", area, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 0), new Vector2(100, 50));
            var trackImg = Img(track, "pill", Hex("3AA04A"), true);
            var knob = Centered("Knob", track, 26, 0, 46, 46);
            Img(knob, "knob", Color.white, false, false);
            var state = Label(area, "State", "Açık", 30, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk, 124, 0, 0, 0);
            var toggle = area.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = trackImg;
            toggle.isOn = true;
            var sw = area.gameObject.AddComponent<ToggleSwitch>();
            Set(sw, "track", trackImg);
            Set(sw, "knob", knob);
            Set(sw, "stateText", state);
            return root.gameObject;
        }

        private static GameObject BuildDropdownRow()
        {
            var root = Row("UI_SettingsRow_Dropdown", "Çözünürlük", out _);
            var resources = new TMP_DefaultControls.Resources
            {
                standard = Sprite("field"),
                background = Sprite("field"),
                inputField = Sprite("field"),
                knob = Sprite("knob"),
                checkmark = Icon("check"),
                dropdown = Icon("arrow_right"),
                mask = Sprite("round_white")
            };
            var dd = TMP_DefaultControls.CreateDropdown(resources);
            dd.name = "Dropdown";
            dd.layer = 5;
            var rt = (RectTransform)dd.transform;
            rt.SetParent(root, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(1f, 0.5f);
            rt.anchoredPosition = new Vector2(-6, 0);
            rt.sizeDelta = new Vector2(430, 58);
            foreach (var t in dd.GetComponentsInChildren<TMP_Text>(true))
            {
                t.font = MenuUiFactory.Font;
                t.fontSharedMaterial = MatInk;
                t.fontSize = 28;
                t.color = Ink;
            }
            var arrow = dd.transform.Find("Arrow") as RectTransform;
            if (arrow != null)
            {
                arrow.localEulerAngles = new Vector3(0, 0, -90);
                arrow.GetComponent<Image>().color = Ink;
                arrow.sizeDelta = new Vector2(26, 26);
            }
            var template = dd.transform.Find("Template") as RectTransform;
            if (template != null)
            {
                template.sizeDelta = new Vector2(0, 300);
                template.GetComponent<Image>().sprite = Sprite("parchment_soft");
                var item = template.Find("Viewport/Content/Item") as RectTransform;
                if (item != null) item.sizeDelta = new Vector2(0, 52);
                var content = template.Find("Viewport/Content") as RectTransform;
                if (content != null) content.sizeDelta = new Vector2(0, 56);
                var itemBg = template.Find("Viewport/Content/Item/Item Background");
                if (itemBg != null) itemBg.GetComponent<Image>().color = new Color(0.96f, 0.82f, 0.45f, 1f);
            }
            var label = dd.transform.Find("Label") as RectTransform;
            if (label != null) { label.offsetMin = new Vector2(18, 4); label.offsetMax = new Vector2(-46, -4); }
            return root.gameObject;
        }

        // ------------------------------------------------------------------ onay, yükleme, logo, tabela
        private static GameObject BuildConfirm()
        {
            var root = Root("UI_ConfirmDialog", 1920, 1080);
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.sizeDelta = Vector2.zero;
            Img(root, (Sprite)null, new Color(0.05f, 0.03f, 0.02f, 0.62f), true);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var window = (GameObject)PrefabUtility.InstantiatePrefab(ParchmentPanel, root);
            var wrt = (RectTransform)window.transform;
            wrt.name = "Window";
            wrt.anchorMin = wrt.anchorMax = wrt.pivot = new Vector2(0.5f, 0.5f);
            wrt.sizeDelta = new Vector2(900, 470);
            var content = wrt.Find("Paper/Content");
            var title = Label(content, "Title", "ÇIKIŞ", 50, Hex("6B3A1E"), TextAlignmentOptions.Top, true, MatInk, 40, 34, 40, 330);
            var message = Label(content, "Message", "Emin misin?", 34, Ink, TextAlignmentOptions.Center, false, MatInk, 60, 110, 60, 150);
            var yes = Inst(WoodPrimary, content, "YesButton", new Vector2(0.5f, 0f), new Vector2(170, 76), new Vector2(330, 100));
            var no = Inst(WoodSecondary, content, "NoButton", new Vector2(0.5f, 0f), new Vector2(-170, 76), new Vector2(330, 96));
            HideIcon(yes);
            HideIcon(no);
            var dialog = root.gameObject.AddComponent<ConfirmDialog>();
            Set(dialog, "titleText", title);
            Set(dialog, "messageText", message);
            Set(dialog, "yesButton", yes.GetComponent<Button>());
            Set(dialog, "noButton", no.GetComponent<Button>());
            Set(dialog, "yesLabel", yes.transform.Find("Label").GetComponent<TMP_Text>());
            Set(dialog, "noLabel", no.transform.Find("Label").GetComponent<TMP_Text>());
            Set(dialog, "window", wrt);
            group.alpha = 0f;
            root.gameObject.SetActive(false);
            return root.gameObject;
        }

        private static GameObject BuildProgress()
        {
            var root = Root("UI_LoadingProgress", 980, 136);
            var frame = Stretch("Frame", root);
            Img(frame, "wood_plank", Color.white);
            var trough = Stretch("Trough", root, 64, 30, 64, 30);
            Img(trough, "bar_trough", Color.white);
            var fillArea = Stretch("FillArea", trough, 8, 8, 8, 8);
            var fill = Node("Fill", fillArea);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0.4f, 1f);
            fill.offsetMin = fill.offsetMax = Vector2.zero;
            Img(fill, "bar_fill", Color.white);
            var leafL = Anchored("LeafL", root, new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(90, -4), new Vector2(64, 64));
            Img(leafL, "leaf", Color.white, false, false);
            var leafR = Anchored("LeafR", root, new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-90, -4), new Vector2(64, 64));
            Img(leafR, "leaf", Color.white, false, false).transform.localScale = new Vector3(-1, 1, 1);
            var percent = Anchored("Percent", root, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-84, 0), new Vector2(100, 50));
            Text(percent, "%0", 26, Cream, TextAlignmentOptions.MidlineRight, true, MatOnWood);
            return root.gameObject;
        }

        private static GameObject BuildLogo()
        {
            var root = Root("UI_BrandLogo", 640, 300);
            // Palas Pandıras logosu (şeffaf PNG); kutudan biraz taşar, oranı korunur.
            var logo = Stretch("Logo", root);
            logo.offsetMin = new Vector2(0, -50);
            logo.offsetMax = new Vector2(0, 40);
            Img(logo, "logo_palas_pandiras", Color.white, false, false);
            return root.gameObject;
        }

        private static void Leaf(RectTransform parent, Vector2 anchor, Vector2 pos, float rot, bool flip = false)
        {
            var leaf = Anchored("Leaf", parent, anchor, new Vector2(0.5f, 0.5f), pos, new Vector2(84, 84));
            Img(leaf, "leaf", Color.white, false, false);
            leaf.localEulerAngles = new Vector3(0, 0, rot + (flip ? 30 : -30));
            if (flip) leaf.localScale = new Vector3(-1, 1, 1);
        }

        private static GameObject BuildTitleSign()
        {
            var root = Root("UI_TitleSign", 820, 176);
            var sign = Stretch("Sign", root);
            Img(sign, "wood_sign", Color.white);
            Leaf(root, new Vector2(0f, 1f), new Vector2(18, -14), 0f);
            Leaf(root, new Vector2(1f, 1f), new Vector2(-18, -14), 0f, true);
            var label = Label(root, "Label", "BAŞLIK", 92, Cream, TextAlignmentOptions.Center, true, MatTitle, 40, 10, 40, 14);
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.enableAutoSizing = true;
            label.fontSizeMin = 50;
            label.fontSizeMax = 96;
            return root.gameObject;
        }

        private static GameObject BuildTaskRow()
        {
            var root = Root("UI_TaskRow", 470, 94);
            root.gameObject.AddComponent<LayoutElement>().preferredHeight = 94;
            var iconBg = Anchored("IconBg", root, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0, 0), new Vector2(84, 84));
            Img(iconBg, "keycap", Color.white);
            var icon = Stretch("Icon", iconBg, 16, 14, 16, 18);
            var iconImg = IconImg(icon, "bookshelf", Hex("6B3A1E"));
            var label = Label(root, "Label", "Görev", 33, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk, 104, 0, 0, 0);
            label.enableAutoSizing = true;
            label.fontSizeMin = 22;
            label.fontSizeMax = 33;
            var view = root.gameObject.AddComponent<TaskRowView>();
            Set(view, "icon", iconImg);
            Set(view, "label", label);
            return root.gameObject;
        }

        private static GameObject BuildHelpRow()
        {
            var root = Root("UI_HelpRow", 560, 96);
            root.gameObject.AddComponent<LayoutElement>().minHeight = 92;
            var h = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 22;
            h.childAlignment = TextAnchor.MiddleLeft;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = h.childForceExpandHeight = false;
            h.padding = new RectOffset(0, 0, 6, 6);
            var keycap = Node("Keycap", root);
            var kl = keycap.gameObject.AddComponent<LayoutElement>();
            kl.preferredWidth = kl.minWidth = 84;
            kl.preferredHeight = kl.minHeight = 80;
            Img(keycap, "keycap", Color.white);
            var key = Label(keycap, "Key", "A", 34, Ink, TextAlignmentOptions.Center, true, MatInk, 6, 4, 6, 12);
            key.textWrappingMode = TextWrappingModes.NoWrap;
            key.enableAutoSizing = true;
            key.fontSizeMin = 18;
            key.fontSizeMax = 34;
            var badge = Node("Badge", root);
            var bl = badge.gameObject.AddComponent<LayoutElement>();
            bl.preferredWidth = bl.minWidth = 70;
            bl.preferredHeight = bl.minHeight = 70;
            var badgeImg = IconImg(badge, "info", Hex("7A4A26"));
            var text = Node("Text", root);
            var tl = text.gameObject.AddComponent<LayoutElement>();
            tl.flexibleWidth = 1;
            var tmp = Text(text, "Açıklama", 29, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk);
            tmp.lineSpacing = -6;
            var view = root.gameObject.AddComponent<HelpRowView>();
            Set(view, "keycap", keycap.gameObject);
            Set(view, "keycapLayout", kl);
            Set(view, "keyText", key);
            Set(view, "badgeIcon", badgeImg);
            Set(view, "text", tmp);
            return root.gameObject;
        }

        // ------------------------------------------------------------------ yardımcılar (sahne kurucusu da kullanır)
        public static GameObject Inst(GameObject prefab, Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return go;
        }

        public static void HideIcon(GameObject button)
        {
            var icon = button.transform.Find("Icon");
            if (icon != null) icon.gameObject.SetActive(false);
            var label = button.transform.Find("Label") as RectTransform;
            if (label != null) { label.offsetMin = new Vector2(24, label.offsetMin.y); label.offsetMax = new Vector2(-24, label.offsetMax.y); }
        }
    }
}
