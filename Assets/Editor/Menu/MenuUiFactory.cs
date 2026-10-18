using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.EditorTools
{
    /// <summary>
    /// Menü kurucusunun ortak yardımcıları: RectTransform yerleşimi, sliced Image, TMP metin (Türkçe dinamik font),
    /// SerializedObject ile private [SerializeField] bağlama. Koordinatlar 1920x1080 referans tuvaline göredir.
    /// </summary>
    public static class MenuUiFactory
    {
        public const string FontDir = MenuArtGenerator.Root + "/Fonts";
        public static readonly Color Ink = Hex("33251C");
        public static readonly Color Cream = Hex("FFF3DA");
        public static readonly Color Parchment = Hex("F8E8C9");
        public static readonly Color Gold = Hex("F4B640");
        public static readonly Color Danger = Hex("B64D3E");
        public static readonly Color Teal = Hex("399C8B");
        public static readonly Color Muted = Hex("7D7265");
        public static readonly Color WoodDark = Hex("503022");

        public static TMP_FontAsset Font;
        public static Material MatTitle, MatOnWood, MatInk;

        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }

        // ------------------------------------------------------------------ font
        public static void EnsureFont()
        {
            System.IO.Directory.CreateDirectory(FontDir);
            var path = FontDir + "/SonDenge_UI SDF.asset";
            Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (Font == null)
            {
                var ttf = AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
                Font = TMP_FontAsset.CreateFontAsset(ttf, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048,
                    AtlasPopulationMode.Dynamic, true);
                Font.name = "SonDenge_UI SDF";
                AssetDatabase.CreateAsset(Font, path);
                Font.material.name = Font.name + " Material";
                AssetDatabase.AddObjectToAsset(Font.material, Font);
                Font.atlasTexture.name = Font.name + " Atlas";
                AssetDatabase.AddObjectToAsset(Font.atlasTexture, Font);
                AssetDatabase.SaveAssets();
            }
            // Türkçe ve sık kullanılan karakterleri önceden ekle (çalışma anında atlas büyümesini azaltır).
            Font.TryAddCharacters("ABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZabcçdefgğhıijklmnoöpqrsştuüvwxyzâÂ0123456789 .,:;!?'\"()-–—/·%+×*#&<>[]_=…✓");
            EditorUtility.SetDirty(Font);
            MatTitle = Preset("SonDenge_UI Title", m =>
            {
                m.SetFloat(ShaderUtilities.ID_FaceDilate, 0.24f);
                m.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.3f);
                m.SetColor(ShaderUtilities.ID_OutlineColor, Hex("3A2212"));
                m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                m.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0.12f, 0.06f, 0.02f, 0.85f));
                m.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.55f);
                m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.75f);
                m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.15f);
                m.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.2f);
            });
            MatOnWood = Preset("SonDenge_UI OnWood", m =>
            {
                m.SetFloat(ShaderUtilities.ID_FaceDilate, 0.14f);
                m.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.14f);
                m.SetColor(ShaderUtilities.ID_OutlineColor, Hex("2A170C"));
                m.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                m.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.5f));
                m.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
                m.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.3f);
            });
            MatInk = Preset("SonDenge_UI Ink", m => m.SetFloat(ShaderUtilities.ID_FaceDilate, 0.04f));
        }

        private static Material Preset(string name, System.Action<Material> setup)
        {
            var path = FontDir + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Font.material) { name = name };
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.CopyPropertiesFromMaterial(Font.material);
            mat.shader = Shader.Find("TextMeshPro/Distance Field");
            mat.shaderKeywords = new string[0];
            setup(mat);
            ShaderUtilities.GetShaderPropertyIDs();
            ShaderUtilities.UpdateShaderRatios(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        // ------------------------------------------------------------------ yerleşim
        public static RectTransform Node(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static RectTransform Stretch(string name, Transform parent, float l = 0, float t = 0, float r = 0, float b = 0)
        {
            var rt = Node(name, parent);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>Üst-sol köşeden piksel konumu (x sağa, y aşağı) ile yerleştirir; pivot üst-sol.</summary>
        public static RectTransform TopLeft(string name, Transform parent, float x, float y, float w, float h)
        {
            var rt = Node(name, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        /// <summary>Ebeveynin merkezine göre (x sağa, y yukarı) yerleştirir.</summary>
        public static RectTransform Centered(string name, Transform parent, float x, float y, float w, float h)
        {
            var rt = Node(name, parent);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Anchored(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = Node(name, parent);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static Sprite Sprite(string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(MenuArtGenerator.SpriteDir + "/" + name + ".png");

        public static Sprite Icon(string name) =>
            AssetDatabase.LoadAssetAtPath<Sprite>(MenuArtGenerator.IconDir + "/icon_" + name + ".png");

        public static Image Img(RectTransform rt, Sprite sprite, Color color, bool raycast = false, bool sliced = true)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = raycast;
            if (sprite != null && sliced && sprite.border != Vector4.zero)
            {
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 1f;
            }
            else image.preserveAspect = !sliced;
            return image;
        }

        public static Image Img(RectTransform rt, string sprite, Color color, bool raycast = false, bool sliced = true) => Img(rt, Sprite(sprite), color, raycast, sliced);

        public static Image IconImg(RectTransform rt, string icon, Color color)
        {
            var i = Img(rt, Icon(icon), color, false, false);
            i.preserveAspect = true;
            return i;
        }

        public static TextMeshProUGUI Text(RectTransform rt, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false, Material mat = null)
        {
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = Font;
            t.fontSharedMaterial = mat != null ? mat : MatInk;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            t.lineSpacing = -8f;
            return t;
        }

        public static TextMeshProUGUI Label(Transform parent, string name, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false, Material mat = null,
            float l = 0, float t = 0, float r = 0, float b = 0)
            => Text(Stretch(name, parent, l, t, r, b), text, size, color, align, bold, mat);

        public static CanvasGroup Group(GameObject go)
        {
            var g = go.GetComponent<CanvasGroup>();
            return g != null ? g : go.AddComponent<CanvasGroup>();
        }

        // ------------------------------------------------------------------ serileştirilmiş alan bağlama
        public static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Menü] Alan yok: " + target.GetType().Name + "." + field); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetArray(Object target, string field, IList<Object> values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Menü] Alan yok: " + target.GetType().Name + "." + field); return; }
            p.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetValue(Object target, string field, System.Action<SerializedProperty> setter)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError("[Menü] Alan yok: " + target.GetType().Name + "." + field); return; }
            setter(p);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static T Child<T>(Transform root, string path) where T : Component
        {
            var t = root.Find(path);
            if (t == null) { Debug.LogError("[Menü] Çocuk yok: " + root.name + "/" + path); return null; }
            return t.GetComponent<T>();
        }
    }
}
