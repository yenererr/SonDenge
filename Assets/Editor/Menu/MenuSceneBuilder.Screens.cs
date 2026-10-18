using System.Collections.Generic;
using Jam.UI.Menu;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static Jam.EditorTools.MenuUiFactory;
using static Jam.EditorTools.MenuPrefabBuilder;

namespace Jam.EditorTools
{
    public static partial class MenuSceneBuilder
    {
        private static readonly Vector2 TL = new Vector2(0f, 1f), TR = new Vector2(1f, 1f), BL = new Vector2(0f, 0f),
            BR = new Vector2(1f, 0f), C = new Vector2(0.5f, 0.5f), TC = new Vector2(0.5f, 1f), BC = new Vector2(0.5f, 0f);

        // ------------------------------------------------------------------ 01 ANA MENÜ
        private static MenuPage BuildMain(Transform pages)
        {
            var rt = Page<MainMenuView>(pages, "01_MainMenu", out var view);
            PageBackdrop(rt, "bg_01_ana_menu");
            // Sol tarafta okunabilirlik gölgesi
            var shade = TopLeft("LeftShade", rt, 0, 0, 900, 1080);
            Img(shade, "soft_shadow", new Color(0.1f, 0.05f, 0.02f, 0.35f));
            Logo(rt, 112, 70, 1.0f);
            var col = TopLeft("Buttons", rt, 160, 420, 560, 520);
            var play = Btn(WoodPrimary, col, "PlayButton", "OYUNA BAŞLA", "play", TL, new Vector2(280, -56), new Vector2(560, 118));
            var how = Btn(WoodSecondary, col, "HowToButton", "NASIL OYNANIR?", "book", TL, new Vector2(280, -188), new Vector2(530, 104));
            var set = Btn(WoodSecondary, col, "SettingsButton", "AYARLAR", "gear", TL, new Vector2(280, -306), new Vector2(530, 104));
            var quit = Btn(WoodSecondary, col, "QuitButton", "ÇIKIŞ", "exit", TL, new Vector2(280, -424), new Vector2(530, 104));
            foreach (var b in new[] { play, how, set, quit })
            {
                var l = b.transform.Find("Label").GetComponent<TMP_Text>();
                l.alignment = TextAlignmentOptions.MidlineLeft;
            }
            var about = Btn(WoodSmall, rt, "AboutButton", "Oyun Hakkında", "info", BL, new Vector2(330, 112), new Vector2(330, 76));
            var version = Anchored("Version", rt, BL, new Vector2(0f, 0f), new Vector2(60, 24), new Vector2(300, 48));
            var vIcon = Anchored("Icon", version, new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(40, 40));
            IconImg(vIcon, "mountain", new Color(1, 1, 1, 0.75f));
            var vText = Label(version, "Text", "v0.1.0", 26, new Color(1, 1, 1, 0.8f), TextAlignmentOptions.MidlineLeft, true, MatOnWood, 54);
            // Wilson + konuşma balonu + slogan levhası
            Wilson(rt, BR, new Vector2(-640, 18), 560);
            var bubble = Bubble(rt, "WilsonBubble", BR, new Vector2(-262, 470), new Vector2(420, 250),
                "Merhaba! Ben Wilson! Afetlere karşı hazırlanmaya var mısın?");
            var sign = Anchored("SloganSign", rt, BR, new Vector2(0.5f, 0.5f), new Vector2(-250, 150), new Vector2(460, 124));
            Img(sign, "wood_plank", Color.white);
            sign.localEulerAngles = new Vector3(0, 0, 5f);
            var post = Anchored("Post", sign, BC, new Vector2(0.5f, 1f), new Vector2(0, 8), new Vector2(26, 140));
            Img(post, "wood_plank", Color.white);
            post.SetAsFirstSibling();
            Label(sign, "Text", "Küçük hazırlıklar\nbüyük fark yaratır!", 30, Cream, TextAlignmentOptions.Center, true, MatOnWood, 24, 10, 24, 14);

            Set(view, "firstSelected", play);
            SetArray(view, "staggered", new Object[] { play.transform, how.transform, set.transform, quit.transform });
            Set(view, "playButton", play);
            Set(view, "howToButton", how);
            Set(view, "settingsButton", set);
            Set(view, "quitButton", quit);
            Set(view, "aboutButton", about);
            Set(view, "playLabel", play.transform.Find("Label").GetComponent<TMP_Text>());
            Set(view, "versionText", vText);
            Set(view, "wilsonText", bubble);
            return view;
        }

        // ------------------------------------------------------------------ 02 BÖLÜM SEÇİMİ
        private static MenuPage BuildChapterSelect(Transform pages)
        {
            var rt = Page<ChapterSelectView>(pages, "02_ChapterSelect", out var view);
            PageBackdrop(rt, "bg_02_bolum_secimi");
            var shade = Stretch("Shade", rt);
            Img(shade, (Sprite)null, new Color(0.08f, 0.04f, 0.1f, 0.25f));
            var back = Btn(WoodSmall, rt, "BackButton", "Ana Menü", "home", TL, new Vector2(230, -84), new Vector2(340, 88));
            // Halatlar + başlık
            foreach (var x in new[] { -250f, 250f })
            {
                var rope = Anchored("Rope", rt, TC, new Vector2(0.5f, 1f), new Vector2(x, 0), new Vector2(12, 60));
                Img(rope, "round_white", Hex("8A6A3A"));
            }
            Title(rt, "BÖLÜM SEÇİMİ", 960, 46, 760, 168);
            var row = TopLeft("Cards", rt, 104, 238, 1712, 584);
            var cards = new List<Object>();
            for (var i = 0; i < 5; i++)
            {
                var card = (GameObject)PrefabUtility.InstantiatePrefab(ChapterCard, row);
                card.name = "Card_" + (i + 1);
                var crt = (RectTransform)card.transform;
                crt.anchorMin = crt.anchorMax = TL;
                crt.pivot = new Vector2(0.5f, 1f);
                crt.anchoredPosition = new Vector2(157 + i * 349.5f, -12);
                cards.Add(card.GetComponent<ChapterCardView>());
            }
            Wilson(rt, BL, new Vector2(190, -6), 330);
            var bubble = Bubble(rt, "WilsonBubble", BL, new Vector2(720, 140), new Vector2(620, 200),
                "İlk olarak deprem bölümünü oynayalım. Evimizi sarsıntıya karşı birlikte hazırlayacağız!");
            var cont = Btn(WoodPrimary, rt, "ContinueButton", "Devam Et", "arrow_right", BR, new Vector2(-370, 134), new Vector2(440, 128), true);
            var nav = cont.navigation;
            nav.mode = Navigation.Mode.None;
            cont.navigation = nav;
            cont.transform.Find("Label").GetComponent<TMP_Text>().fontSize = 56;
            SetArray(view, "cards", cards);
            Set(view, "firstSelected", cont);
            Set(view, "continueButton", cont);
            Set(view, "continueLabel", cont.transform.Find("Label").GetComponent<TMP_Text>());
            Set(view, "backButton", back);
            Set(view, "wilsonText", bubble);
            return view;
        }

        // ------------------------------------------------------------------ 03 WILSON GÖREV TANITIMI
        private static MenuPage BuildBriefing(Transform pages)
        {
            var rt = Page<BriefingView>(pages, "03_Briefing", out var view);
            PageBackdrop(rt, "bg_03_brifing_ic", 0.12f);
            Logo(rt, 50, 34, 0.55f);
            var sign = (GameObject)PrefabUtility.InstantiatePrefab(TitleSign, rt);
            sign.name = "TitleSign";
            var srt = (RectTransform)sign.transform;
            srt.anchorMin = srt.anchorMax = TC;
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0, -40);
            srt.sizeDelta = new Vector2(700, 236);
            var main = srt.Find("Label").GetComponent<TMP_Text>();
            main.text = "DEPREM";
            var mrt = (RectTransform)main.transform;
            mrt.offsetMin = new Vector2(40, 10);
            mrt.offsetMax = new Vector2(-40, -86);
            main.fontSizeMax = 120;
            var number = Label(srt, "Number", "1. BÖLÜM", 56, Cream, TextAlignmentOptions.Top, true, MatTitle, 40, 20, 40, 140);
            Wilson(rt, BL, new Vector2(300, -10), 600);
            var bubble = Bubble(rt, "WilsonBubble", TL, new Vector2(640, -440), new Vector2(480, 290),
                "Gıdak! Bu ev biraz sallanabilir. Önce ağır eşyaları güvenli hâle getirelim!", false, 90);
            var back = Btn(WoodSmall, rt, "BackButton", "Bölüm Seçimine Dön", "arrow_left", TR, new Vector2(-250, -84), new Vector2(420, 88));

            var content = Panel(rt, "TaskPanel", 860, 300, 980, 620);
            var heading = Label(content, "Heading", "Bu Bölümdeki Görevler", 50, Hex("4A2414"), TextAlignmentOptions.TopLeft, true, MatInk, 40, 30, 40, 0);
            var divider = TopLeft("Divider", content, 30, 96, 870, 34);
            Img(divider, "divider", Color.white).type = Image.Type.Simple;
            var list = TopLeft("TaskList", content, 40, 140, 470, 420);
            var v = list.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 6;
            v.childControlHeight = true;
            v.childControlWidth = true;
            v.childForceExpandHeight = false;
            // UNUTMA kartı
            var card = TopLeft("SafetyCard", content, 520, 130, 400, 330);
            Img(card, "parchment_soft", Color.white);
            var warn = Anchored("Icon", card, TL, new Vector2(0.5f, 0.5f), new Vector2(46, -46), new Vector2(56, 56));
            IconImg(warn, "exclamation", Danger);
            Label(card, "Title", "Unutma:", 36, Danger, TextAlignmentOptions.TopLeft, true, MatInk, 86, 22, 10, 0);
            var safety = Label(card, "Text", "", 26, Ink, TextAlignmentOptions.TopLeft, false, MatInk, 26, 76, 22, 150);
            safety.enableAutoSizing = true;
            safety.fontSizeMin = 18;
            safety.fontSizeMax = 26;
            var strip = TopLeft("DropCoverHold", card, 20, 186, 360, 136);
            var labels = new[] { "ÇÖK", "KAPAN", "TUTUN" };
            for (var i = 0; i < 3; i++)
            {
                var cell = TopLeft("Step" + (i + 1), strip, i * 122, 0, 112, 136);
                var box = TopLeft("Box", cell, 0, 0, 112, 96);
                Img(box, "keycap", Color.white);
                var pic = Stretch("Pic", box, 12, 10, 12, 16);
                IconImg(pic, "drop_cover_" + (i + 1), Ink);
                Label(cell, "Label", labels[i], 24, Ink, TextAlignmentOptions.Bottom, true, MatInk, 0, 96, 0, 0);
            }
            var safetyIcon = Anchored("DisasterIcon", card, BC, new Vector2(0.5f, 0f), new Vector2(0, 28), new Vector2(110, 110));
            var safetyImg = IconImg(safetyIcon, "bolt", Hex("7A4A26"));
            var start = Btn(WoodPrimary, rt, "StartButton", "Hazırlığa Başla", "play", TL, new Vector2(1470, -912), new Vector2(600, 132));
            start.transform.Find("Label").GetComponent<TMP_Text>().fontSize = 54;

            Set(view, "firstSelected", start);
            Set(view, "chapterNumberText", number);
            Set(view, "chapterNameText", main);
            Set(view, "wilsonText", bubble);
            Set(view, "taskList", list);
            Set(view, "taskRowPrefab", TaskRow.GetComponent<TaskRowView>());
            Set(view, "fallbackTaskIcon", Icon("check"));
            Set(view, "safetyText", safety);
            Set(view, "dropCoverStrip", strip.gameObject);
            Set(view, "safetyIcon", safetyImg);
            Set(view, "startButton", start);
            Set(view, "backButton", back);
            _ = heading;
            return view;
        }

        // ------------------------------------------------------------------ 04 YÜKLEME
        private static LoadingView BuildLoading(Transform pages)
        {
            var rt = Page<LoadingView>(pages, "04_Loading", out var view);
            PageBackdrop(rt, "bg_04_yukleme");
            Logo(rt, 120, 110, 1.0f);
            var bar = (GameObject)PrefabUtility.InstantiatePrefab(LoadingProgress, rt);
            bar.name = "Progress";
            var brt = (RectTransform)bar.transform;
            brt.anchorMin = brt.anchorMax = TC;
            brt.pivot = new Vector2(0.5f, 0.5f);
            brt.anchoredPosition = new Vector2(0, -852);
            var status = Anchored("Status", rt, TC, new Vector2(0.5f, 0.5f), new Vector2(0, -960), new Vector2(1100, 60));
            var statusText = Text(status, "Bölüm yükleniyor...", 38, Cream, TextAlignmentOptions.Center, true, MatOnWood);
            Wilson(rt, BL, new Vector2(240, 216), 290);
            var tip = (GameObject)PrefabUtility.InstantiatePrefab(TipCard, rt);
            tip.name = "TipCard";
            var trt = (RectTransform)tip.transform;
            trt.anchorMin = trt.anchorMax = BL;
            trt.pivot = new Vector2(0f, 0f);
            trt.anchoredPosition = new Vector2(36, 44);
            trt.sizeDelta = new Vector2(470, 220);
            Set(view, "fill", brt.Find("Trough/FillArea/Fill"));
            Set(view, "statusText", statusText);
            Set(view, "percentText", brt.Find("Percent").GetComponent<TMP_Text>());
            Set(view, "tipTitle", trt.Find("Title").GetComponent<TMP_Text>());
            Set(view, "tipText", trt.Find("Text").GetComponent<TMP_Text>());
            WriteTips(view);
            return view;
        }

        // ------------------------------------------------------------------ 05 NASIL OYNANIR
        private static MenuPage BuildHelp(Transform pages)
        {
            var rt = Page<HowToPlayView>(pages, "05_HowToPlay", out var view);
            PageBackdrop(rt, "bg_05_nasil_oynanir_ic", 0.12f);
            Logo(rt, 60, 30, 0.55f);
            Title(rt, "NASIL OYNANIR?", 960, 56, 820, 168);
            var frameGo = (GameObject)PrefabUtility.InstantiatePrefab(ParchmentPanel, rt);
            frameGo.name = "HelpPanel";
            var frt = (RectTransform)frameGo.transform;
            frt.anchorMin = frt.anchorMax = TL;
            frt.pivot = new Vector2(0f, 1f);
            frt.anchoredPosition = new Vector2(100, -232);
            frt.sizeDelta = new Vector2(1600, 740);
            var paper = frt.Find("Paper");
            paper.GetComponent<Image>().enabled = false; // sekme sütunu ahşap üstünde, içerik ayrı parşömende
            var content = paper.Find("Content");
            var tabCol = TopLeft("Tabs", content, 14, 14, 330, 660);
            Img(tabCol, "parchment_soft", new Color(1, 1, 1, 0.92f));
            var tv = tabCol.gameObject.AddComponent<VerticalLayoutGroup>();
            tv.padding = new RectOffset(14, 14, 16, 16);
            tv.spacing = 14;
            tv.childControlHeight = false;
            tv.childControlWidth = true;
            tv.childForceExpandHeight = false;
            var tabs = new List<Object>();
            for (var i = 0; i < 6; i++)
            {
                var tab = (GameObject)PrefabUtility.InstantiatePrefab(TabButton, tabCol);
                tab.name = "Tab" + (i + 1);
                ((RectTransform)tab.transform).sizeDelta = new Vector2(300, 90);
                tabs.Add(tab.GetComponent<TabButtonView>());
            }
            var sheet = TopLeft("Sheet", content, 360, 0, 1188, 688);
            Img(sheet, "parchment", Color.white);
            var heading = Label(sheet, "Heading", "Temel Kontroller", 54, Hex("4A2414"), TextAlignmentOptions.TopLeft, true, MatInk, 56, 40, 40, 0);
            var div = TopLeft("Divider", sheet, 50, 112, 900, 34);
            Img(div, "divider", Color.white).type = Image.Type.Simple;
            var scrollRt = TopLeft("Scroll", sheet, 50, 160, 940, 500);
            var scroll = scrollRt.gameObject.AddComponent<ScrollRect>();
            var viewport = Stretch("Viewport", scrollRt);
            viewport.gameObject.AddComponent<RectMask2D>();
            Img(viewport, (Sprite)null, new Color(0, 0, 0, 0), true);
            var body = Node("Body", viewport);
            body.anchorMin = new Vector2(0, 1);
            body.anchorMax = new Vector2(1, 1);
            body.pivot = new Vector2(0.5f, 1f);
            body.sizeDelta = Vector2.zero;
            var hl = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 40;
            hl.childControlWidth = hl.childControlHeight = true;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = false;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            RectTransform Column(string name)
            {
                var col = Node(name, body);
                var cv = col.gameObject.AddComponent<VerticalLayoutGroup>();
                cv.spacing = 12;
                cv.childControlWidth = cv.childControlHeight = true;
                cv.childForceExpandHeight = false;
                return col;
            }
            var left = Column("Left");
            var right = Column("Right");
            scroll.viewport = viewport;
            scroll.content = body;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            Wilson(rt, BR, new Vector2(-230, -6), 470, true);
            var bubble = Bubble(rt, "WilsonBubble", TR, new Vector2(-230, -350), new Vector2(380, 230),
                "Kontrolleri öğrenirsen her şey daha kolay olacak!", false, 120);
            var back = Btn(WoodSmall, rt, "BackButton", "Geri", "arrow_left", BL, new Vector2(270, 54), new Vector2(300, 84));
            SetArray(view, "tabs", tabs);
            Set(view, "firstSelected", back);
            Set(view, "heading", heading);
            Set(view, "leftColumn", left);
            Set(view, "rightColumn", right);
            Set(view, "scroll", scroll);
            Set(view, "rowPrefab", HelpRow.GetComponent<HelpRowView>());
            Set(view, "wilsonText", bubble);
            Set(view, "backButton", back);
            WriteHelp(view);
            return view;
        }

        // ------------------------------------------------------------------ 06 AYARLAR
        private static SettingsView BuildSettings(Transform pages, ConfirmDialog confirm, bool embedded = false)
        {
            var rt = Page<SettingsView>(pages, "06_Settings", out var view);
            if (embedded) Img(Stretch("Dim", rt), (Sprite)null, new Color(0.05f, 0.03f, 0.02f, 0.55f), true);
            else
            {
                PageBackdrop(rt, "bg_06_ayarlar");
                Logo(rt, 60, 70, 0.72f);
                Wilson(rt, BL, new Vector2(200, 40), 430);
            }
            var content = Panel(rt, "SettingsPanel", 420, 288, 1300, 690);
            Title(rt, "AYARLAR", 1070, 150, 560, 160);
            var catCol = TopLeft("Categories", content, 22, 22, 330, 620);
            Img(catCol, "parchment_soft", new Color(1, 1, 1, 0.75f));
            var cv = catCol.gameObject.AddComponent<VerticalLayoutGroup>();
            cv.padding = new RectOffset(12, 12, 14, 14);
            cv.spacing = 12;
            cv.childControlHeight = false;
            cv.childControlWidth = true;
            cv.childForceExpandHeight = false;
            string[] names = { "Ses", "Görüntü", "Oynanış", "Dil", "Erişilebilirlik" };
            string[] icons = { "speaker", "monitor", "gear", "globe", "person" };
            var tabs = new List<Object>();
            for (var i = 0; i < names.Length; i++)
            {
                var tab = (GameObject)PrefabUtility.InstantiatePrefab(TabButton, catCol);
                tab.name = "Tab_" + names[i];
                ((RectTransform)tab.transform).sizeDelta = new Vector2(300, 86);
                var tv = tab.GetComponent<TabButtonView>();
                tab.transform.Find("Label").GetComponent<TMP_Text>().text = names[i];
                tab.transform.Find("Icon").GetComponent<Image>().sprite = Icon(icons[i]);
                tabs.Add(tv);
            }
            var back = Btn(WoodSmall, catCol, "BackButton", "Geri", "arrow_left", BC, new Vector2(0, 52), new Vector2(290, 80));
            back.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var title = Label(content, "PanelTitle", "Görüntü Ayarları", 42, Hex("4A2414"), TextAlignmentOptions.TopLeft, true, MatInk, 390, 30, 30, 0);
            var line = TopLeft("TitleLine", content, 380, 92, 850, 3);
            Img(line, (Sprite)null, new Color(0.45f, 0.3f, 0.18f, 0.6f));
            RectTransform PanelArea(string name)
            {
                var p = TopLeft(name, content, 380, 108, 860, 420);
                var v = p.gameObject.AddComponent<VerticalLayoutGroup>();
                v.spacing = 0;
                v.childControlWidth = true;
                v.childControlHeight = true;
                v.childForceExpandHeight = false;
                return p;
            }
            GameObject RowOf(GameObject prefab, Transform parent, string caption)
            {
                var r = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                r.name = caption;
                r.transform.Find("Label").GetComponent<TMP_Text>().text = caption;
                return r;
            }
            // Ses
            var sound = PanelArea("Panel_Ses");
            var master = RowOf(SettingsSliderRow, sound, "Ana Ses");
            var music = RowOf(SettingsSliderRow, sound, "Müzik");
            var sfx = RowOf(SettingsSliderRow, sound, "Efekt Sesleri");
            var mute = RowOf(SettingsToggleRow, sound, "Sesi Kapat");
            mute.transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
            // Görüntü
            var display = PanelArea("Panel_Goruntu");
            var res = RowOf(SettingsDropdownRow, display, "Çözünürlük");
            var full = RowOf(SettingsToggleRow, display, "Tam Ekran");
            var vs = RowOf(SettingsToggleRow, display, "VSync");
            var qual = RowOf(SettingsDropdownRow, display, "Grafik Kalitesi");
            // Oynanış
            var game = PanelArea("Panel_Oynanis");
            var shake = RowOf(SettingsToggleRow, game, "Kamera Sarsıntısını Azalt");
            shake.transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
            var resetRow = Node("İlerleme", game);
            resetRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 120;
            Label(resetRow, "Label", "Bölüm ilerlemesi (yıldızlar ve kilitler)", 30, Ink, TextAlignmentOptions.MidlineLeft, true, MatInk, 12, 0, 380, 0);
            var reset = Btn(WoodSmall, resetRow, "ResetProgress", "Sıfırla", "exclamation", new Vector2(1f, 0.5f), new Vector2(-180, 0), new Vector2(340, 80));
            Note(game, "Kamera sarsıntısı depremde ve fırtınada ekranı sallar. Hassassan azalt.");
            // Dil
            var lang = PanelArea("Panel_Dil");
            var langRow = Node("Dil", lang);
            langRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 76;
            Label(langRow, "Label", "Oyun Dili", 32, Ink, TextAlignmentOptions.MidlineLeft, true, MatInk, 12, 0, 470, 0);
            var langBox = Anchored("Value", langRow, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-6, 0), new Vector2(430, 58));
            Img(langBox, "field", Color.white);
            Label(langBox, "Text", "Türkçe (varsayılan)", 28, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk, 18);
            Note(lang, "Son Denge şu anda yalnız Türkçe oynanabilir. Başka dil eklendiğinde burada seçilebilecek.");
            // Erişilebilirlik
            var access = PanelArea("Panel_Erisilebilirlik");
            var motion = RowOf(SettingsToggleRow, access, "Hareketleri Azalt");
            motion.transform.Find("Toggle").GetComponent<Toggle>().isOn = false;
            var shake2 = RowOf(SettingsToggleRow, access, "Ekran Sarsıntısını Azalt (oyun)");
            shake2.SetActive(false); // aynı ayar Oynanış'ta; çift kontrol oluşturmamak için gizli
            Note(access, "Hareketleri azalt: menü geçişleri kısalır, arka plan kayması ve Wilson'ın zıplaması durur. Bütün yazılar Türkçe ve altyazı gerektirmez.");

            var status = Label(content, "Status", "", 24, Hex("3A7A3A"), TextAlignmentOptions.BottomLeft, true, MatInk, 390, 0, 30, 140);
            var defaults = Btn(WoodSecondary, content, "DefaultsButton", "Varsayılana Dön", null, BL, new Vector2(580, 70), new Vector2(380, 96));
            defaults.GetComponent<Image>().sprite = Sprite("parchment_soft");
            var dl = defaults.transform.Find("Label").GetComponent<TMP_Text>();
            dl.color = Ink;
            dl.fontSharedMaterial = MatInk;
            var apply = Btn(WoodPrimary, content, "ApplyButton", "Uygula", null, BL, new Vector2(1010, 70), new Vector2(400, 100));

            Set(view, "confirm", confirm);
            SetArray(view, "tabs", tabs);
            SetArray(view, "panels", new Object[] { sound.gameObject, display.gameObject, game.gameObject, lang.gameObject, access.gameObject });
            Set(view, "panelTitle", title);
            Set(view, "master", master.transform.Find("Slider").GetComponent<Slider>());
            Set(view, "music", music.transform.Find("Slider").GetComponent<Slider>());
            Set(view, "sfx", sfx.transform.Find("Slider").GetComponent<Slider>());
            Set(view, "masterValue", master.transform.Find("Value").GetComponent<TMP_Text>());
            Set(view, "musicValue", music.transform.Find("Value").GetComponent<TMP_Text>());
            Set(view, "sfxValue", sfx.transform.Find("Value").GetComponent<TMP_Text>());
            Set(view, "mute", mute.transform.Find("Toggle").GetComponent<Toggle>());
            Set(view, "resolution", res.transform.Find("Dropdown").GetComponent<TMP_Dropdown>());
            Set(view, "fullscreen", full.transform.Find("Toggle").GetComponent<Toggle>());
            Set(view, "vsync", vs.transform.Find("Toggle").GetComponent<Toggle>());
            Set(view, "quality", qual.transform.Find("Dropdown").GetComponent<TMP_Dropdown>());
            Set(view, "reduceShake", shake.transform.Find("Toggle").GetComponent<Toggle>());
            Set(view, "resetProgress", reset);
            Set(view, "reduceMotion", motion.transform.Find("Toggle").GetComponent<Toggle>());
            Set(view, "applyButton", apply);
            Set(view, "defaultsButton", defaults);
            Set(view, "backButton", back);
            Set(view, "statusText", status);
            Set(view, "firstSelected", apply);
            return view;
        }

        private static void Note(Transform parent, string text)
        {
            var n = Node("Note", parent);
            n.gameObject.AddComponent<LayoutElement>().preferredHeight = 110;
            Label(n, "Text", text, 25, Muted, TextAlignmentOptions.TopLeft, false, MatInk, 12, 18, 12, 0);
        }

        // ------------------------------------------------------------------ 07 OYUN HAKKINDA
        private static MenuPage BuildAbout(Transform pages)
        {
            var rt = Page<AboutView>(pages, "07_About", out var view);
            PageBackdrop(rt, "bg_07_hakkinda");
            var frameContent = Panel(rt, "AboutPanel", 322, 232, 1552, 660);
            frameContent.parent.GetComponent<Image>().enabled = false;
            Title(rt, "OYUN HAKKINDA", 960, 62, 860, 172);
            var left = TopLeft("Story", frameContent, 0, 0, 1130, 608);
            Img(left, "parchment", Color.white);
            Logo(left, 70, 40, 0.5f);
            Label(left, "Description",
                "Son Denge, sivri bir dağın zirvesindeki fantastik evin dengesini korurken gerçek afet hazırlıklarını öğrendiğin fizik ve strateji oyunudur. Eşyalarını güvenli yerleştir, görevleri tamamla, afetlere hazırlıklı ol.",
                28, Ink, TextAlignmentOptions.TopLeft, false, MatInk, 50, 210, 640, 90);
            Label(left, "Safety", "Ev dengeleme sistemi kurgusaldır; gerçek afetlerde resmî güvenlik yönergelerini izleyin.",
                22, Hex("8A3A2A"), TextAlignmentOptions.BottomLeft, true, MatInk, 50, 0, 640, 30);
            var picFrame = TopLeft("HousePicture", left, 520, 34, 580, 540);
            Img(picFrame, "parchment_soft", Color.white);
            var pic = Stretch("Image", picFrame, 18, 18, 18, 18);
            var raw = pic.gameObject.AddComponent<RawImage>();
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BgDir + "/bg_07_hakkinda.png");
            raw.uvRect = new Rect(0.52f, 0.24f, 0.44f, 0.44f * 1672f / 941f * 504f / 544f);
            raw.raycastTarget = false;
            var side = TopLeft("Side", frameContent, 1150, 0, 350, 608);
            Img(side, "parchment", Color.white);
            var header = TopLeft("Header", side, 14, 14, 322, 92);
            Img(header, "wood_plank", Color.white);
            var sideTitle = Label(header, "Text", "Ekibimiz", 40, Cream, TextAlignmentOptions.Center, true, MatOnWood, 10, 6, 10, 10);
            var team = TopLeft("Team", side, 20, 120, 310, 470);
            var tv = team.gameObject.AddComponent<VerticalLayoutGroup>();
            tv.spacing = 4;
            tv.childControlHeight = true;
            tv.childControlWidth = true;
            tv.childForceExpandHeight = false;
            (string icon, string text)[] roles =
            {
                ("code", "Yener Er\nLead · Çekirdek, Menü, Bölüm 1"),
                ("palette", "Emre Gündoğdu\nSon Denge ekibi"),
                ("cube", "Tülay Aktenli\nSon Denge ekibi"),
                ("image", "Saygın Çamsoy\nSon Denge ekibi"),
                ("clipboard", "Oyun Testi\nBütün ekip")
            };
            foreach (var role in roles)
            {
                var r = Node("Role", team);
                r.gameObject.AddComponent<LayoutElement>().preferredHeight = 90;
                var ic = Anchored("Icon", r, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(6, 0), new Vector2(50, 50));
                IconImg(ic, role.icon, Hex("4A2A18"));
                Label(r, "Text", role.text, 24, Ink, TextAlignmentOptions.MidlineLeft, false, MatInk, 72);
            }
            var credits = TopLeft("Credits", side, 18, 118, 316, 474);
            var scroll = credits.gameObject.AddComponent<ScrollRect>();
            var viewport = Stretch("Viewport", credits);
            viewport.gameObject.AddComponent<RectMask2D>();
            Img(viewport, (Sprite)null, new Color(0, 0, 0, 0), true);
            var body = Node("Body", viewport);
            body.anchorMin = new Vector2(0, 1);
            body.anchorMax = new Vector2(1, 1);
            body.pivot = new Vector2(0.5f, 1f);
            body.sizeDelta = Vector2.zero;
            var ct = Text(body, CreditsText, 20, Ink, TextAlignmentOptions.TopLeft, false, MatInk);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            ct.lineSpacing = 0;
            scroll.viewport = viewport;
            scroll.content = body;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 30;
            credits.gameObject.SetActive(false);
            var thanks = Btn(WoodSecondary, rt, "ThanksButton", "Teşekkürler", "heart", TL, new Vector2(860, -948), new Vector2(380, 104));
            var back = Btn(WoodPrimary, rt, "BackButton", "Geri", "back_triangle", TL, new Vector2(1330, -948), new Vector2(380, 108));
            Wilson(rt, BL, new Vector2(170, 20), 430);
            Set(view, "thanksButton", thanks);
            Set(view, "thanksLabel", thanks.transform.Find("Label").GetComponent<TMP_Text>());
            Set(view, "backButton", back);
            Set(view, "teamPanel", team.gameObject);
            Set(view, "creditsPanel", credits.gameObject);
            Set(view, "sideTitle", sideTitle);
            Set(view, "creditsScroll", scroll);
            Set(view, "firstSelected", back);
            return view;
        }

        private const string CreditsText =
            "<b>Wilson</b>\nLow Poly Chicken — Pjager (CC BY 4.0)\nYumurta — Casca_Servillius (CC BY 4.0)\n\n" +
            "<b>Ev ve eşyalar</b>\nMedieval Room Kit — 1Luka2\nDOLLHOUSE Modular Low Poly Interiors — ForgeWorks\n" +
            "Stylized Tavern Prop Pack — VEE-TREX\nLowPolyInteriorsFree, 3Digitalis Interior Props, StylArts, FarmHouse, " +
            "Modular Sewer Props (Ata Khani), Roadside Tales Fences (EmaceArt) — Unity Asset Store Standart EULA\n" +
            "KayKit Adventurers — Kay Lousberg (CC0)\n\n" +
            "<b>Yazı tipi</b>\nLiberation Sans (SIL Open Font License)\n\n" +
            "<b>Menü arayüzü</b>\nAhşap/parşömen görselleri, ikonlar ve arayüz sesleri proje içinde kodla üretildi.\n\n" +
            "Ayrıntılı liste: README › Atıflar\n\nOyunumuzu deneyen herkese teşekkürler!";

        // ------------------------------------------------------------------ veri
        private static void WriteChapters(MenuFlowController flow)
        {
            var data = new[]
            {
                new { sub = "İlk Sarsıntı", ribbon = "B4692E", tint = "FFFFFF", over = new Color(1f, 0.6f, 0.2f, 0.12f), icon = "mountain",
                    select = "Deprem bölümünde evimizi sarsıntıya karşı birlikte hazırlayacağız!",
                    brief = "Gıdak! Bu ev biraz sallanabilir. Önce ağır eşyaları güvenli hâle getirelim!",
                    safety = "Deprem sırasında Çök–Kapan–Tutun. Eşya taşımaya çalışma.",
                    tasks = new[] { "bookshelf", "fridge", "medkit", "scale" }, dch = true },
                new { sub = "Rüzgâra Karşı", ribbon = "2F5FA8", tint = "8EA6D6", over = new Color(0.1f, 0.2f, 0.5f, 0.38f), icon = "roof",
                    select = "Fırtına geliyor! Çatıyı, pencereleri ve balkonu birlikte güçlendirelim.",
                    brief = "Gıdak! Rüzgâr geliyor. Çatıyı, pencereleri ve balkonu kontrol edelim!",
                    safety = "Şiddetli rüzgârda pencerelerden uzak, güvenli bir iç alanda kal.",
                    tasks = new[] { "roof", "window", "box" }, dch = false },
                new { sub = "Dumanın Ardında", ribbon = "C0392B", tint = "FFB48C", over = new Color(0.95f, 0.3f, 0.05f, 0.32f), icon = "extinguisher",
                    select = "Yangın çıkmadan önlem alalım: alarm, söndürücü ve açık bir kaçış yolu!",
                    brief = "Gıdak! Yangın çıkmadan önlem alalım. Alarmı, tüpü ve kaçış yolunu hazırlayalım!",
                    safety = "Yoğun dumanda yangınla mücadeleye girişme; güvenli tahliye önceliklidir.",
                    tasks = new[] { "smoke_alarm", "stove", "extinguisher", "door" }, dch = false },
                new { sub = "Gökyüzünün Öfkesi", ribbon = "6A3FA0", tint = "A890D8", over = new Color(0.35f, 0.12f, 0.6f, 0.4f), icon = "bolt",
                    select = "Gök gürlüyor! Elektrik güvenliğini fırtınadan önce tamamlayalım.",
                    brief = "Gıdak! Gök gürültüsü yaklaşmadan elektrik güvenliği hazırlıklarını tamamlayalım!",
                    safety = "Gök gürlerken dışarı çıkma ve elektrikli cihazların fişini çekme.",
                    tasks = new[] { "shield", "plug", "bolt" }, dch = false },
                new { sub = "Son Denge", ribbon = "1F6F8B", tint = "84B4CC", over = new Color(0.05f, 0.3f, 0.5f, 0.38f), icon = "drop",
                    select = "Son bölüm! Suyun eve girmemesi için birlikte hazırlanalım.",
                    brief = "Gıdak! Çok yağmur yağacak! Suyun eve girmemesi için hazırlanalım!",
                    safety = "Su baskınında elektrikli cihazlardan uzak dur; güvenli yere geç.",
                    tasks = new[] { "sandbag", "drop", "arrow_up", "shield" }, dch = false }
            };
            SetValue(flow, "chapters", p =>
            {
                p.arraySize = data.Length;
                for (var i = 0; i < data.Length; i++)
                {
                    var d = data[i];
                    var e = p.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("subtitle").stringValue = d.sub;
                    e.FindPropertyRelative("ribbonColor").colorValue = Hex(d.ribbon);
                    e.FindPropertyRelative("thumbnailTint").colorValue = Hex(d.tint);
                    e.FindPropertyRelative("overlayTint").colorValue = d.over;
                    e.FindPropertyRelative("disasterIcon").objectReferenceValue = Icon(d.icon);
                    e.FindPropertyRelative("selectLine").stringValue = d.select;
                    e.FindPropertyRelative("briefingLine").stringValue = d.brief;
                    e.FindPropertyRelative("safetyNote").stringValue = d.safety;
                    e.FindPropertyRelative("showDropCoverHold").boolValue = d.dch;
                    var icons = e.FindPropertyRelative("taskIcons");
                    icons.arraySize = d.tasks.Length;
                    for (var k = 0; k < d.tasks.Length; k++) icons.GetArrayElementAtIndex(k).objectReferenceValue = Icon(d.tasks[k]);
                }
            });
        }

        private static void WriteTips(LoadingView view)
        {
            (bool real, string text)[] tips =
            {
                (false, "Ağır eşyaları evin ortasına yakın yerleştirmek evin dengesini korur."),
                (false, "Büyük eşyayı seçip F ile sabitle; sabit eşya afette kaymaz."),
                (false, "E ile küçük eşyayı al, hedef alanın yanında yeniden E ile bırak."),
                (false, "Denge göstergesi kırmızıya yaklaşırsa ağırlığı karşı tarafa taşı."),
                (false, "Ev 26°'yi yaklaşık 1 saniye aşarsa devrilir. Eğimi izle!"),
                (false, "Görevler bitince BOŞLUK ile afete erken başlayabilirsin."),
                (false, "Depremde BOŞLUK ile Çök–Kapan–Tutun yap; doğru davranış puan kazandırır."),
                (false, "Fare tekeriyle yakınlaş, Home tuşuyla kadrajı sıfırla."),
                (true, "Dolapları ve kitaplıkları önceden duvara sabitle."),
                (true, "Deprem anında koşma; Çök–Kapan–Tutun."),
                (true, "Deprem çantanı evdeki herkesin bildiği bir yerde tut."),
                (true, "Yangında dumanın altında kal ve güvenle tahliye ol."),
                (true, "Duman alarmını düzenli olarak test et."),
                (true, "Gök gürlerken dışarı çıkma; elektrikli cihazların fişini önceden çek."),
                (true, "Su basan alanda elektriğe dokunma, yüksek ve güvenli bir yere geç."),
                (true, "Fırtınada pencerelerden uzak, iç bir odada kal.")
            };
            SetValue(view, "tips", p =>
            {
                p.arraySize = tips.Length;
                for (var i = 0; i < tips.Length; i++)
                {
                    p.GetArrayElementAtIndex(i).FindPropertyRelative("realLife").boolValue = tips[i].real;
                    p.GetArrayElementAtIndex(i).FindPropertyRelative("text").stringValue = tips[i].text;
                }
            });
        }

        private static void WriteHelp(HowToPlayView view)
        {
            var sections = new[]
            {
                ("Temel Kontroller", "gamepad", "Temel Kontroller", "Kontrolleri öğrenirsen her şey daha kolay olacak!", new (string, string, string)[]
                {
                    ("A / D", null, "Sola / sağa yürü"), ("W / S", null, "Merdivenden çık / in"),
                    ("E", null, "Küçük eşyayı al / bırak, görev noktasını kullan"), ("F", null, "Seçili büyük eşyayı sabitle"),
                    (null, "mouse", "Sol tık: eşyayı seç ve sürükle · Sağ tık: iptal"), ("BOŞLUK", null, "Afete hazırım · depremde Çök–Kapan–Tutun"),
                    (null, "mouse", "Tekerlek: yakınlaş / uzaklaş · Home: kadrajı sıfırla"), ("ESC", null, "Oyunu duraklat")
                }),
                ("Görevler", "clipboard", "Görevler", "Her görev evimizi biraz daha güvenli yapar!", new (string, string, string)[]
                {
                    (null, "info", "Her bölüm Wilson'ın brifingiyle başlar; hazırlık süresi sen başlayınca akar."),
                    (null, "clipboard", "Görev listesi ekranın köşesinde; tamamlanan görev işaretlenir."),
                    (null, "box", "Taşınacak eşyayı E ile al, parlayan hedef alana bırak."),
                    (null, "lock", "Sabitleme görevinde eşyayı seç ve F'ye bas."),
                    (null, "bulb", "Takılırsan Wilson'dan ipucu iste."),
                    (null, "clock", "Süre biterse afet eksik görevlerle başlar.")
                }),
                ("Denge Sistemi", "scale", "Denge Sistemi", "Ağırlıkları dengeli dağıtırsan ev sapasağlam kalır!", new (string, string, string)[]
                {
                    (null, "mountain", "Ev sivri bir kayanın tepesinde durur."),
                    (null, "scale", "Her eşyanın etkisi: kütle × merkeze yatay uzaklık."),
                    (null, "arrow_right", "Ağırlık bir yana yığılırsa ev o yöne eğilir."),
                    (null, "exclamation", "26°'yi yaklaşık 1 saniye aşarsa ev devrilir."),
                    (null, "info", "Bu denge kuralı oyuna özgü ve kurgusaldır; gerçek evler böyle çalışmaz.")
                }),
                ("Afet Aşamaları", "mountain", "Afet Aşamaları", "Hazırlık, afet, rapor... sonra yeni bölüm!", new (string, string, string)[]
                {
                    ("1", null, "Hazırlık: görevleri tamamla, eşyaları yerleştir."),
                    ("2", null, "Afet: 12–30 saniye sürer, doğru davranışı seç."),
                    ("3", null, "Afet Raporu: puan, yıldız ve öğrendiklerin."),
                    (null, "shield", "Hazırlık önlemleri afetteki hasarı azaltır."),
                    (null, "clock", "Süre bitince afet eksik görevlerle başlar.")
                }),
                ("Puanlama", "trophy", "Puanlama", "Üç yıldız için kritik görevleri unutma!", new (string, string, string)[]
                {
                    ("35", null, "Hazırlık görevleri"), ("35", null, "Afette doğru davranış"),
                    ("20", null, "Ev dengesi"), ("10", null, "Eşyaları koruma"),
                    (null, "star", "70+ puan 2 yıldız · 90+ ve bütün kritik görevler 3 yıldız"),
                    (null, "trophy", "Ev devrilirse 0 yıldız; en iyi yıldızın kaydedilir.")
                }),
                ("İpuçları", "bulb", "İpuçları", "Bunlar gerçek hayatta da işe yarar!", new (string, string, string)[]
                {
                    (null, "lock", "Ağır mobilyayı önceden duvara sabitle."),
                    (null, "person", "Depremde koşma; Çök–Kapan–Tutun."),
                    (null, "door", "Yangında kaçış yolunu açık tut, dumanın altında kal."),
                    (null, "bolt", "Yıldırım tehlikesinde içeride kal."),
                    (null, "drop", "Su baskınında elektrikli cihazlardan uzak dur."),
                    (null, "info", "Oyundaki eşya taşıma gücü kurgudur; gerçek afette eşya taşıma.")
                })
            };
            SetValue(view, "sections", p =>
            {
                p.arraySize = sections.Length;
                for (var i = 0; i < sections.Length; i++)
                {
                    var s = sections[i];
                    var e = p.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("tabTitle").stringValue = s.Item1;
                    e.FindPropertyRelative("tabIcon").objectReferenceValue = Icon(s.Item2);
                    e.FindPropertyRelative("heading").stringValue = s.Item3;
                    e.FindPropertyRelative("wilsonLine").stringValue = s.Item4;
                    var items = e.FindPropertyRelative("items");
                    items.arraySize = s.Item5.Length;
                    for (var k = 0; k < s.Item5.Length; k++)
                    {
                        var it = items.GetArrayElementAtIndex(k);
                        it.FindPropertyRelative("key").stringValue = s.Item5[k].Item1 ?? "";
                        it.FindPropertyRelative("icon").objectReferenceValue = s.Item5[k].Item2 != null ? Icon(s.Item5[k].Item2) : null;
                        it.FindPropertyRelative("text").stringValue = s.Item5[k].Item3;
                    }
                }
            });
        }
    }
}
