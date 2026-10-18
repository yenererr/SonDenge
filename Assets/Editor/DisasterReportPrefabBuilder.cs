using Jam.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.EditorTools
{
    /// <summary>
    /// "Dağ Günbatımında Oyun Sonuç Paneli" görselini arka plan şablonu olarak kullanan, beş bölümün
    /// ortak AFET RAPORU prefab'ını kurar. Bütün yerleşim görselin piksel koordinatlarından
    /// normalize anchor'larla yapılır; görsel hangi çözünürlükte ölçeklenirse UI onunla birlikte kalır.
    /// Menü: Jam > Wilson > Afet raporu prefab'ını kur
    /// </summary>
    public static class DisasterReportPrefabBuilder
    {
        private const string ArtDir = "Assets/Levels/Level1/Art/UI";
        private const string TemplatePath = ArtDir + "/L1_UI_DisasterReportTemplate.png";
        private const string SourcePath = ArtDir + "/L1_UI_DisasterReportTemplate_Source.png";
        private const string StarOnPath = ArtDir + "/L1_UI_Star_Active.png";
        private const string StarOffPath = ArtDir + "/L1_UI_Star_Inactive.png";
        public const string PrefabPath = "Assets/Levels/Level1/Prefabs/UI_DisasterReportTemplate.prefab";
        private const float W = 1672f, H = 941f;

        private static readonly Color Ink = new Color32(40, 42, 44, 255);
        private static readonly Color Wood = new Color32(156, 106, 67, 255);
        private static readonly Color Green = new Color32(78, 150, 125, 255);
        private static readonly Color Cream = new Color32(250, 243, 228, 255);

        [MenuItem("Jam/Wilson/Afet raporu prefab'ını kur")]
        public static GameObject Build()
        {
            ConfigureSprite(TemplatePath);
            ConfigureSprite(SourcePath);
            ConfigureSprite(StarOnPath);
            ConfigureSprite(StarOffPath);
            AssetDatabase.Refresh();
            var template = AssetDatabase.LoadAssetAtPath<Sprite>(TemplatePath);
            var starOn = AssetDatabase.LoadAssetAtPath<Sprite>(StarOnPath);
            var starOff = AssetDatabase.LoadAssetAtPath<Sprite>(StarOffPath);

            var root = new GameObject("UI_DisasterReportTemplate", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 75;
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
            var group = root.AddComponent<CanvasGroup>();
            var panel = root.AddComponent<DisasterReportPanel>();

            // Arka katman: tam ekran karartma (16:9 dışı ekranlarda kenar boşluklarını doldurur, tıklamaları yutar).
            var shade = Stretch(root.transform, "Karartma");
            var shadeImg = shade.gameObject.AddComponent<Image>();
            shadeImg.color = new Color(0.07f, 0.05f, 0.05f, 0.85f);

            // Şablon görsel: 16:9 en-boy oranı korunarak ekrana sığar (FitInParent: kart hiçbir zaman kesilmez).
            var bg = Stretch(root.transform, "Şablon (arka plan)");
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.sprite = template;
            bgImg.preserveAspect = false;
            bgImg.raycastTarget = true;
            var fitter = bg.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = W / H;

            // UI katmanı: şablonla aynı dikdörtgen; bütün çocuklar görsel piksel koordinatlarıyla yerleşir.
            var ui = Stretch(bg, "UI katmanı");

            var title = Text(ui, "Başlık", "AFET RAPORU", 52, Ink, TextAlignmentOptions.Center, FontStyles.Bold);
            Place(title.rectTransform, 520, 100, 1152, 158);
            var chapter = Text(ui, "Bölüm adı", "BÖLÜM 01 · İLK SARSINTI", 27, Wood, TextAlignmentOptions.Center, FontStyles.Bold);
            Place(chapter.rectTransform, 520, 156, 1152, 190);
            var status = Text(ui, "Durum", "EV AYAKTA KALDI", 30, Green, TextAlignmentOptions.Center, FontStyles.Bold);
            Place(status.rectTransform, 90, 28, 472, 84);
            var score = Text(ui, "Toplam puan", "PUAN  <b>0</b> / 100", 30, Ink, TextAlignmentOptions.Center);
            Place(score.rectTransform, 1362, 28, 1616, 84);

            // Üst orta çubuk: şablondaki denge çubuğu, en yüksek eğimi gösteren canlı işaretçiyle.
            var track = Rect(ui, "Eğim çubuğu");
            Place(track, 652, 46, 1020, 82);
            var marker = Rect(track, "En yüksek eğim işareti");
            marker.sizeDelta = new Vector2(10f, 14f);
            var markerImg = marker.gameObject.AddComponent<Image>();
            markerImg.color = Ink;
            markerImg.raycastTarget = false;
            var tiltLabel = Text(ui, "Eğim yazısı", "EN YÜKSEK EĞİM  0.0°", 18, Cream, TextAlignmentOptions.Center, FontStyles.Bold);
            Place(tiltLabel.rectTransform, 652, 46, 1020, 82);
            tiltLabel.outlineWidth = 0.25f;
            tiltLabel.outlineColor = new Color32(40, 30, 22, 255);

            var stars = new Image[3];
            float[] cx = { 720f, 836f, 952f }, cy = { 238f, 226f, 238f }, size = { 96f, 112f, 96f };
            for (var i = 0; i < 3; i++)
            {
                var s = Rect(ui, "Yıldız " + (i + 1));
                Place(s, cx[i] - size[i] / 2f, cy[i] - size[i] / 2f, cx[i] + size[i] / 2f, cy[i] + size[i] / 2f);
                stars[i] = s.gameObject.AddComponent<Image>();
                stars[i].sprite = starOff;
                stars[i].preserveAspect = true;
                stars[i].raycastTarget = false;
            }

            // Sol: dört kategori satırı (şablondaki dört kutu).
            string[] names = { "Hazırlık görevleri", "Doğru afet davranışı", "Denge", "Eşya koruma" };
            float[] rowTop = { 298f, 373f, 448f, 522f };
            var values = new TMP_Text[4];
            for (var i = 0; i < 4; i++)
            {
                var row = Rect(ui, "Puan satırı — " + names[i]);
                Place(row, 252, rowTop[i], 596, rowTop[i] + 60);
                var label = Text(row, "Ad", names[i], 24, Ink, TextAlignmentOptions.MidlineLeft);
                Fill(label.rectTransform);
                values[i] = Text(row, "Değer", "0 / 0", 27, Ink, TextAlignmentOptions.MidlineRight, FontStyles.Bold);
                Fill(values[i].rectTransform);
            }

            // Sağ: dinamik madde listesi.
            var list = Rect(ui, "Madde listesi");
            Place(list, 690, 314, 1416, 574);
            var layout = list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            var proto = Text(list, "Madde (şablon)", "● Madde", 22, Ink, TextAlignmentOptions.TopLeft);
            proto.textWrappingMode = TextWrappingModes.Normal;

            // Wilson alanı.
            var portraitRt = Rect(ui, "Wilson portresi");
            Place(portraitRt, 252, 618, 361, 715);
            var portrait = portraitRt.gameObject.AddComponent<RawImage>();
            portrait.raycastTarget = false;
            portrait.enabled = false;
            var message = Text(ui, "Wilson mesajı", "<b>WILSON:</b> ...", 27, Ink, TextAlignmentOptions.MidlineLeft);
            Place(message.rectTransform, 386, 616, 1416, 668);
            message.textWrappingMode = TextWrappingModes.Normal;
            var note = Text(ui, "Gerçek hayat notu", "", 19, Green, TextAlignmentOptions.TopLeft, FontStyles.Italic);
            Place(note.rectTransform, 386, 668, 1416, 722);
            note.textWrappingMode = TextWrappingModes.Normal;

            // Düğmeler: şablondaki üç çerçevenin üstünde gerçek Button bileşenleri.
            var left = MakeButton(ui, "Düğme — sol", 244, 752, 604, 830, "BU BÖLÜMÜ TEKRAR DENE", 25, Ink, out var leftLabel);
            var primary = MakeButton(ui, "Düğme — orta (vurgulu)", 650, 752, 1022, 830, "SONRAKİ BÖLÜM", 31, Cream, out var primaryLabel);
            var right = MakeButton(ui, "Düğme — sağ", 1066, 752, 1430, 830, "ANA MENÜ", 25, Ink, out var rightLabel);

            var so = new SerializedObject(panel);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("chapterText").objectReferenceValue = chapter;
            so.FindProperty("statusText").objectReferenceValue = status;
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("tiltTrack").objectReferenceValue = track;
            so.FindProperty("tiltMarker").objectReferenceValue = marker;
            so.FindProperty("tiltText").objectReferenceValue = tiltLabel;
            var starArr = so.FindProperty("starImages");
            starArr.arraySize = 3;
            for (var i = 0; i < 3; i++) starArr.GetArrayElementAtIndex(i).objectReferenceValue = stars[i];
            so.FindProperty("starActive").objectReferenceValue = starOn;
            so.FindProperty("starInactive").objectReferenceValue = starOff;
            var catArr = so.FindProperty("categoryValues");
            catArr.arraySize = 4;
            for (var i = 0; i < 4; i++) catArr.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.FindProperty("lineContainer").objectReferenceValue = list;
            so.FindProperty("linePrototype").objectReferenceValue = proto;
            so.FindProperty("wilsonPortrait").objectReferenceValue = portrait;
            so.FindProperty("wilsonMessageText").objectReferenceValue = message;
            so.FindProperty("safetyNoteText").objectReferenceValue = note;
            so.FindProperty("leftButton").objectReferenceValue = left;
            so.FindProperty("primaryButton").objectReferenceValue = primary;
            so.FindProperty("rightButton").objectReferenceValue = right;
            so.FindProperty("leftLabel").objectReferenceValue = leftLabel;
            so.FindProperty("primaryLabel").objectReferenceValue = primaryLabel;
            so.FindProperty("rightLabel").objectReferenceValue = rightLabel;
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[Wilson] " + PrefabPath + " kuruldu.");
            return prefab;
        }

        private static void ConfigureSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
        }

        // ---------------------------------------------------------------- yerleşim yardımcıları
        private static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static RectTransform Stretch(Transform parent, string name)
        {
            var rt = Rect(parent, name);
            Fill(rt);
            return rt;
        }

        private static void Fill(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Şablon görselindeki piksel dikdörtgenine (sol-üst orijin) normalize anchor ile yerleştirir.</summary>
        private static void Place(RectTransform rt, float x0, float y0, float x1, float y1)
        {
            rt.anchorMin = new Vector2(x0 / W, 1f - y1 / H);
            rt.anchorMax = new Vector2(x1 / W, 1f - y0 / H);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align, FontStyles style = FontStyles.Normal)
        {
            var rt = Rect(parent, name);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.NoWrap;
            t.overflowMode = TextOverflowModes.Ellipsis;
            t.enableAutoSizing = true;
            t.fontSizeMin = Mathf.Max(12f, size * 0.6f);
            t.fontSizeMax = size;
            return t;
        }

        private static Button MakeButton(RectTransform parent, string name, float x0, float y0, float x1, float y1,
            string caption, float fontSize, Color textColor, out TMP_Text label)
        {
            var rt = Rect(parent, name);
            Place(rt, x0, y0, x1, y1);
            // Şablondaki çerçeve görünür kalır; üstte yalnız tıklanabilir alan ve basma/üzerine gelme vurgusu.
            var hit = rt.gameObject.AddComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            var glow = Rect(rt, "Vurgu");
            Fill(glow);
            glow.offsetMin = new Vector2(8f, 8f);
            glow.offsetMax = new Vector2(-8f, -8f);
            var glowImg = glow.gameObject.AddComponent<Image>();
            glowImg.color = Color.white;
            glowImg.raycastTarget = false;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = glowImg;
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.16f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0f);
            colors.pressedColor = new Color(0f, 0f, 0f, 0.14f);
            colors.disabledColor = new Color(0.2f, 0.15f, 0.1f, 0.35f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            rt.gameObject.AddComponent<UiHoverScale>();
            label = Text(rt, "Yazı", caption, fontSize, textColor, TextAlignmentOptions.Center, FontStyles.Bold);
            Fill(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(14f, 6f);
            label.rectTransform.offsetMax = new Vector2(-14f, -6f);
            return button;
        }
    }
}
