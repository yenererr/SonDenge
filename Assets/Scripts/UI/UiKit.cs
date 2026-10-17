using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Jam.UI
{
    /// <summary>
    /// Kodla uGUI kurmak için küçük yardımcılar (Canvas 1920x1080, Match 0.5, Input System UI modülü).
    /// Menü ve bölüm HUD'ları aynı sıcak kâğıt/ahşap paletini paylaşsın diye tek yerde tutulur.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color Cream = new Color32(246, 236, 214, 255);
        public static readonly Color Paper = new Color32(238, 224, 196, 245);
        public static readonly Color Wood = new Color32(96, 62, 38, 255);
        public static readonly Color Ink = new Color32(58, 40, 28, 255);
        public static readonly Color Teal = new Color32(74, 140, 132, 255);
        public static readonly Color Orange = new Color32(201, 116, 58, 255);
        public static readonly Color Olive = new Color32(112, 128, 66, 255);
        public static readonly Color Mustard = new Color32(214, 168, 62, 255);
        public static readonly Color Coral = new Color32(196, 74, 60, 255);

        private static Font defaultFont;

        public static Font DefaultFont
        {
            get
            {
                if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return defaultFont;
            }
        }

        public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        /// <summary>Sahnede EventSystem yoksa Input System UI modülüyle bir tane kurar.</summary>
        public static void EnsureEventSystem(Transform parent)
        {
            if (EventSystem.current != null || Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.transform.SetParent(parent, false);
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(Transform parent, string name)
        {
            var rt = Rect(parent, name, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return rt;
        }

        public static Image Panel(RectTransform rt, Color fill, bool raycast = false)
        {
            var image = rt.gameObject.AddComponent<Image>();
            image.color = fill;
            image.raycastTarget = raycast;
            return image;
        }

        public static Outline Border(Graphic graphic, Color color, float width = 3f)
        {
            var outline = graphic.gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, -width);
            return outline;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var rt = Stretch(parent, name);
            var label = rt.gameObject.AddComponent<Text>();
            label.font = DefaultFont;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = anchor;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            return label;
        }

        public static Button Button(RectTransform rt, string caption, int fontSize, Color fill, Color textColor,
            UnityEngine.Events.UnityAction onClick)
        {
            var image = Panel(rt, fill, true);
            Border(image, Wood, 2f);
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.06f, 1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.82f, 0.78f, 1f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            colors.fadeDuration = 0.14f;
            button.colors = colors;
            rt.gameObject.AddComponent<UiHoverScale>();
            var label = Label(rt, "Yazı", caption, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.rectTransform.offsetMin = new Vector2(8f, 2f);
            label.rectTransform.offsetMax = new Vector2(-8f, -2f);
            if (onClick != null) button.onClick.AddListener(onClick);
            return button;
        }

        public static Slider Slider(RectTransform rt, float value, UnityEngine.Events.UnityAction<float> onChange)
        {
            var background = Rect(rt, "Arka", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(0f, 10f));
            Panel(background, new Color32(120, 96, 72, 255));
            var fillArea = Rect(rt, "Dolgu alanı", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(-20f, 10f));
            var fill = Rect(fillArea, "Dolgu", Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, Vector2.zero);
            Panel(fill, Teal);
            var handleArea = Rect(rt, "Tutamak alanı", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(-20f, 0f));
            var handle = Rect(handleArea, "Tutamak", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(22f, 0f));
            var handleImage = Panel(handle, Cream, true);
            Border(handleImage, Wood, 2f);
            var slider = rt.gameObject.AddComponent<Slider>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            if (onChange != null) slider.onValueChanged.AddListener(onChange);
            return slider;
        }

        public static Toggle Toggle(RectTransform rt, string caption, bool value, UnityEngine.Events.UnityAction<bool> onChange)
        {
            var box = Rect(rt, "Kutu", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero,
                new Vector2(30f, 30f));
            var boxImage = Panel(box, Cream, true);
            Border(boxImage, Wood, 2f);
            var check = Rect(box, "İşaret", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-10f, -10f));
            var checkImage = Panel(check, Teal);
            var label = Label(rt, "Yazı", caption, 26, Ink);
            label.rectTransform.offsetMin = new Vector2(44f, 0f);
            var toggle = rt.gameObject.AddComponent<Toggle>();
            toggle.targetGraphic = boxImage;
            toggle.graphic = checkImage;
            toggle.isOn = value;
            if (onChange != null) toggle.onValueChanged.AddListener(onChange);
            return toggle;
        }
    }

    /// <summary>Fare üzerine gelince düğmeyi hafifçe büyütür (0.12–0.18 sn).</summary>
    public sealed class UiHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float speed = 12f;
        private float target = 1f;

        public void OnPointerEnter(PointerEventData eventData) => target = hoverScale;
        public void OnPointerExit(PointerEventData eventData) => target = 1f;

        private void OnDisable()
        {
            target = 1f;
            transform.localScale = Vector3.one;
        }

        private void Update()
        {
            var s = Mathf.Lerp(transform.localScale.x, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(s, s, 1f);
        }
    }
}
