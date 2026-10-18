using System.Collections.Generic;
using Jam.Missions;
using Jam.UI.Menu;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static Jam.EditorTools.MenuUiFactory;
using static Jam.EditorTools.MenuPrefabBuilder;

namespace Jam.EditorTools
{
    /// <summary>
    /// Menu.unity içindeki yedi ekranlı menüyü gerçek uGUI/TMP nesneleriyle kurar (referanslar docs/design/menu_references).
    /// Eski "[Menu] Ana Menü" (MainMenuUI) nesnesi kaldırılır; kamera, ışık ve diğer nesneler korunur.
    /// Tekrar çalıştırılabilir: önceki "[MenuUI]" kökü silinip yeniden kurulur. Menü: Jam > Menü > Menü sahnesini kur
    /// </summary>
    public static partial class MenuSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Menu.unity";
        public const string BgDir = "Assets/Prefabs/UI/Menu/Art/Backgrounds";
        private const string Backdrop = BgDir + "/bg_01_ana_menu.png";
        private const string Cozy = "Assets/Levels/Level1/Art/L1_CozyPreview.png";
        private const string WilsonDir = "Assets/Levels/Level1/Art/Characters/Wilson";
        private static readonly string[] LevelAssets =
        {
            "Assets/Data/Levels/L1_Deprem.asset", "Assets/Data/Levels/L2_Firtina.asset", "Assets/Data/Levels/L3_Yangin.asset",
            "Assets/Data/Levels/L4_Yildirim.asset", "Assets/Data/Levels/L5_Yagmur.asset"
        };

        private static readonly List<RawImage> WilsonTargets = new List<RawImage>();

        [MenuItem("Jam/Menü/Hepsini kur (sanat + prefab + sahne)")]
        public static void BuildEverything()
        {
            MenuArtGenerator.GenerateAll();
            BuildPrefabsAndScene();
        }

        [MenuItem("Jam/Menü/Menü sahnesini kur")]
        public static void BuildPrefabsAndScene()
        {
            EnsureFont();
            BuildAll();
            BuildScene();
        }

        private static void BuildScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == "[MenuUI]" || go.name == "[Menu] Ana Menü") Object.DestroyImmediate(go);
            WilsonTargets.Clear();

            var root = new GameObject("[MenuUI]");
            var canvasGo = new GameObject("MenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.layer = 5;
            canvasGo.transform.SetParent(root.transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var c = (RectTransform)canvasGo.transform;
            var audio = canvasGo.AddComponent<MenuAudioBridge>();
            Set(audio, "click", Clip("ui_click"));
            Set(audio, "hover", Clip("ui_hover"));
            Set(audio, "open", Clip("ui_open"));
            Set(audio, "confirm", Clip("ui_confirm"));
            Set(audio, "error", Clip("ui_error"));

            // EventSystem: sahnede yoksa tek bir tane (Input System UI modülü).
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.transform.SetParent(root.transform, false);
            }

            // Ortak arka plan (oyunun ev + gün batımı render'ı, yazısız) + okunabilirlik katmanı
            var bg = Stretch("Backdrop", c);
            var bgRaw = bg.gameObject.AddComponent<RawImage>();
            ImportBackground(Backdrop);
            bgRaw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Backdrop);
            bgRaw.raycastTarget = false;
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;
            bg.gameObject.AddComponent<BackdropDrift>();
            var vignette = Stretch("Vignette", c);
            Img(vignette, "vignette", new Color(1, 1, 1, 0.9f)).type = Image.Type.Simple;

            var pages = Stretch("Pages", c);
            var flowGo = root;
            var flow = flowGo.AddComponent<MenuFlowController>();

            var main = BuildMain(pages);
            var select = BuildChapterSelect(pages);
            var briefing = BuildBriefing(pages);
            var loading = BuildLoading(pages);
            var help = BuildHelp(pages);
            var confirm = (GameObject)PrefabUtility.InstantiatePrefab(ConfirmDialogPrefab, c);
            confirm.name = "ConfirmDialog";
            var settings = BuildSettings(pages, confirm.GetComponent<ConfirmDialog>());
            var about = BuildAbout(pages);
            confirm.transform.SetAsLastSibling();

            // Wilson: tek portre kamerası, bütün sayfalar aynı RenderTexture'ı gösterir.
            var wilsonGo = new GameObject("[MenuWilson]");
            wilsonGo.transform.SetParent(root.transform, false);
            var wilson = wilsonGo.AddComponent<MenuWilson>();
            Set(wilson, "chickenModel", AssetDatabase.LoadAssetAtPath<GameObject>(WilsonDir + "/low_poly_chicken.obj"));
            Set(wilson, "chickenTexture", AssetDatabase.LoadAssetAtPath<Texture2D>(WilsonDir + "/low_poly_chicken.png"));
            SetArray(wilson, "targets", WilsonTargets.ConvertAll(r => (Object)r));

            SetValue(flow, "pages", p =>
            {
                var list = new (MenuPageId, MenuPage)[]
                {
                    (MenuPageId.Main, main), (MenuPageId.ChapterSelect, select), (MenuPageId.Briefing, briefing),
                    (MenuPageId.Loading, loading), (MenuPageId.Help, help), (MenuPageId.Settings, settings), (MenuPageId.About, about)
                };
                p.arraySize = list.Length;
                for (var i = 0; i < list.Length; i++)
                {
                    var e = p.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("id").enumValueIndex = (int)list[i].Item1;
                    e.FindPropertyRelative("page").objectReferenceValue = list[i].Item2;
                }
            });
            Set(flow, "confirm", confirm.GetComponent<ConfirmDialog>());
            Set(flow, "menuAudio", audio);
            Set(flow, "wilson", wilson);
            Set(flow, "loading", loading);
            Set(flow, "menuMusic", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Levels/Level1/Audio/bgm_cozy_mountain.mp3"));
            var levels = new List<Object>();
            foreach (var path in LevelAssets) levels.Add(AssetDatabase.LoadAssetAtPath<LevelDefinition>(path));
            SetArray(flow, "levels", levels);
            WriteChapters(flow);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Menü] Menu.unity kuruldu: 7 ekran, " + WilsonTargets.Count + " Wilson görünümü.");
        }

        // ------------------------------------------------------------------ oyun içi Duraklat menüsü
        public const string PausePrefabPath = MenuPrefabBuilder.Dir + "/UI_PauseMenu.prefab";
        private static readonly string[] LevelScenes =
            { "Assets/Scenes/Level1.unity", "Assets/Scenes/Level2.unity", "Assets/Scenes/Level3.unity", "Assets/Scenes/Level4.unity", "Assets/Scenes/Level5.unity" };

        [MenuItem("Jam/Menü/Duraklat menüsünü kur ve bölümlere bağla")]
        public static void BuildPauseAndWire()
        {
            EnsureFont();
            BuildAll();
            var prefab = BuildPausePrefab();
            var active = EditorSceneManager.GetActiveScene().path;
            foreach (var path in LevelScenes)
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                var director = Object.FindAnyObjectByType<Jam.Levels.Level1.L1_MissionDirector>();
                if (director == null) { Debug.LogWarning("[Menü] L1_MissionDirector yok: " + path); continue; }
                Set(director, "pauseMenuPrefab", prefab.GetComponent<PauseMenuView>());
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            if (!string.IsNullOrEmpty(active)) EditorSceneManager.OpenScene(active, OpenSceneMode.Single);
            Debug.Log("[Menü] UI_PauseMenu 5 bölüme bağlandı.");
        }

        private static GameObject BuildPausePrefab()
        {
            WilsonTargets.Clear();
            var go = new GameObject("UI_PauseMenu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)) { layer = 5 };
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            var audio = go.AddComponent<MenuAudioBridge>();
            Set(audio, "click", Clip("ui_click"));
            Set(audio, "hover", Clip("ui_hover"));
            Set(audio, "open", Clip("ui_open"));
            Set(audio, "confirm", Clip("ui_confirm"));
            Set(audio, "error", Clip("ui_error"));
            var c = (RectTransform)go.transform;
            var root = Stretch("Root", c);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            Img(root, (Sprite)null, new Color(0.06f, 0.03f, 0.02f, 0.6f), true);
            var window = Centered("Window", root, 0, -10, 680, 720);
            var paperGo = (GameObject)PrefabUtility.InstantiatePrefab(ParchmentPanel, window);
            var prt = (RectTransform)paperGo.transform;
            prt.anchorMin = Vector2.zero;
            prt.anchorMax = Vector2.one;
            prt.offsetMin = new Vector2(0, 0);
            prt.offsetMax = new Vector2(0, -70);
            var sign = (GameObject)PrefabUtility.InstantiatePrefab(TitleSign, window);
            var srt = (RectTransform)sign.transform;
            srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 1f);
            srt.pivot = new Vector2(0.5f, 1f);
            srt.anchoredPosition = new Vector2(0, 10);
            srt.sizeDelta = new Vector2(640, 150);
            srt.Find("Label").GetComponent<TMP_Text>().text = "OYUN DURAKLATILDI";
            var col = new Vector2(0.5f, 1f);
            var resume = Btn(WoodPrimary, window, "ResumeButton", "DEVAM ET", "play", col, new Vector2(0, -230), new Vector2(500, 108));
            var restart = Btn(WoodSecondary, window, "RestartButton", "YENİDEN BAŞLAT", "arrow_up", col, new Vector2(0, -352), new Vector2(480, 98));
            restart.transform.Find("Icon").localEulerAngles = new Vector3(0, 0, 135);
            var settingsBtn = Btn(WoodSecondary, window, "SettingsButton", "AYARLAR", "gear", col, new Vector2(0, -464), new Vector2(480, 98));
            var menuBtn = Btn(WoodSecondary, window, "MenuButton", "ANA MENÜ", "home", col, new Vector2(0, -576), new Vector2(480, 98));
            var confirm = (GameObject)PrefabUtility.InstantiatePrefab(ConfirmDialogPrefab, c);
            confirm.name = "ConfirmDialog";
            var settings = BuildSettings(c, confirm.GetComponent<ConfirmDialog>(), true);
            confirm.transform.SetAsLastSibling();
            var view = go.AddComponent<PauseMenuView>();
            Set(view, "root", group);
            Set(view, "window", window);
            Set(view, "resumeButton", resume);
            Set(view, "restartButton", restart);
            Set(view, "settingsButton", settingsBtn);
            Set(view, "menuButton", menuBtn);
            Set(view, "confirm", confirm.GetComponent<ConfirmDialog>());
            Set(view, "settings", settings);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, PausePrefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(MenuArtGenerator.AudioDir + "/" + name + ".wav");

        // ------------------------------------------------------------------ ortak parçalar
        private static RectTransform Page<T>(Transform parent, string name, out T view) where T : MenuPage
        {
            var rt = Stretch(name, parent);
            rt.gameObject.AddComponent<CanvasGroup>();
            view = rt.gameObject.AddComponent<T>();
            return rt;
        }

        private static RawImage Wilson(Transform parent, Vector2 anchor, Vector2 pos, float size, bool mirror = false)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(WilsonPortraitFrame, parent);
            go.name = "Wilson";
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(size, size);
            if (mirror) rt.localScale = new Vector3(-1, 1, 1);
            var raw = rt.Find("Render").GetComponent<RawImage>();
            WilsonTargets.Add(raw);
            return raw;
        }

        private static TMP_Text Bubble(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size, string text,
            bool tailRight = false, float tailX = 46f)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(WilsonMessage, parent);
            go.name = name;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tail = (RectTransform)rt.Find("Tail");
            tail.anchoredPosition = new Vector2(tailX, 64);
            if (tailRight)
            {
                tail.anchorMin = tail.anchorMax = new Vector2(1f, 0f);
                tail.anchoredPosition = new Vector2(-tailX, 64);
                tail.localScale = new Vector3(-1, 1, 1);
            }
            var t = rt.Find("Body/Text").GetComponent<TMP_Text>();
            t.text = text;
            return t;
        }

        private static GameObject Logo(Transform parent, float x, float y, float scale)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(BrandLogo, parent);
            go.name = "BrandLogo";
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.localScale = Vector3.one * scale;
            return go;
        }

        private static TMP_Text Title(Transform parent, string text, float centerX, float top, float w, float h)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(TitleSign, parent);
            go.name = "TitleSign";
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(centerX - 960f, -top);
            rt.sizeDelta = new Vector2(w, h);
            var label = rt.Find("Label").GetComponent<TMP_Text>();
            label.text = text;
            return label;
        }

        private static Button Btn(GameObject prefab, Transform parent, string name, string caption, string icon,
            Vector2 anchor, Vector2 pos, Vector2 size, bool iconRight = false)
        {
            var go = Inst(prefab, parent, name, anchor, pos, size);
            go.transform.Find("Label").GetComponent<TMP_Text>().text = caption;
            if (string.IsNullOrEmpty(icon)) HideIcon(go);
            else
            {
                go.transform.Find("Icon").GetComponent<Image>().sprite = Icon(icon);
                if (iconRight)
                {
                    var ic = (RectTransform)go.transform.Find("Icon");
                    ic.anchorMin = ic.anchorMax = new Vector2(1f, 0.5f);
                    ic.anchoredPosition = new Vector2(-size.y * 0.75f, 0f);
                    var label = (RectTransform)go.transform.Find("Label");
                    label.offsetMin = new Vector2(size.y * 0.5f, label.offsetMin.y);
                    label.offsetMax = new Vector2(-size.y * 1.15f, label.offsetMax.y);
                }
            }
            return go.GetComponent<Button>();
        }

        private static RectTransform Panel(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(ParchmentPanel, parent);
            go.name = name;
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return (RectTransform)rt.Find("Paper/Content");
        }

        /// <summary>Sayfaya özel, yazısız arka plan katmanı (en altta; arkadaki ortak katmanı tıklamaya kapatır).</summary>
        private static RawImage PageBackdrop(Transform parent, string file, float warmShade = 0f)
        {
            ImportBackground(BgDir + "/" + file + ".png");
            var holder = Stretch("Background", parent);
            holder.SetAsFirstSibling();
            holder.gameObject.AddComponent<RectMask2D>();
            var bg = Stretch("Image", holder);
            var raw = bg.gameObject.AddComponent<RawImage>();
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BgDir + "/" + file + ".png");
            raw.raycastTarget = true;
            var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = 16f / 9f;
            bg.gameObject.AddComponent<BackdropDrift>();
            if (warmShade > 0f) Img(Stretch("WarmShade", holder), (Sprite)null, new Color(0.2f, 0.1f, 0.03f, warmShade));
            Img(Stretch("Vignette", holder), "vignette", new Color(1, 1, 1, 0.75f)).type = Image.Type.Simple;
            return raw;
        }

        public static void ImportBackground(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if (importer == null || (importer.textureType == TextureImporterType.Default && !importer.mipmapEnabled && importer.maxTextureSize == 2048)) return;
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static RawImage InteriorBackdrop(Transform parent, Rect uv)
        {
            var bg = Stretch("InteriorBackdrop", parent);
            var raw = bg.gameObject.AddComponent<RawImage>();
            raw.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Cozy);
            raw.uvRect = uv;
            raw.color = new Color(0.92f, 0.82f, 0.72f);
            raw.raycastTarget = true; // arkadaki ortak manzarayı tıklamaya kapatır
            var warm = Stretch("WarmShade", parent);
            Img(warm, (Sprite)null, new Color(0.22f, 0.11f, 0.04f, 0.38f));
            var vig = Stretch("Vignette", parent);
            Img(vig, "vignette", Color.white).type = Image.Type.Simple;
            return raw;
        }
    }
}
