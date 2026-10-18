using Jam.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Sıradaki görevin yerini ekranda zıplayan bir okla gösterir; karakter hedefe yaklaşınca
    /// hangi tuşa basılacağını yazar. Hedef ekran dışındaysa ok kenarda hedef yönünü gösterir.
    /// Hedef ve ipucu metnini L1_MissionDirector her kare verir (tek doğruluk kaynağı görev sırası).
    /// </summary>
    public sealed class L1_TaskGuide : MonoBehaviour
    {
        [SerializeField] private float bobPixels = 14f;
        [SerializeField] private float edgeMargin = 70f;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.9f, 0f);

        private RectTransform canvasRect, arrowRoot, promptRoot;
        private Text arrow, prompt;
        private CanvasGroup group;
        private Camera cam;

        public static L1_TaskGuide Create(Transform parent)
        {
            var canvas = UiKit.CreateCanvas(parent, "Görev oku", 25);
            var guide = canvas.gameObject.AddComponent<L1_TaskGuide>();
            guide.Build((RectTransform)canvas.transform);
            return guide;
        }

        private void Build(RectTransform root)
        {
            canvasRect = root;
            group = root.gameObject.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            arrowRoot = UiKit.Rect(root, "Ok", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(90f, 90f));
            arrow = UiKit.Label(arrowRoot, "İşaret", "▼", 78, new Color32(255, 196, 64, 255), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiKit.Border(arrow, new Color32(70, 40, 20, 255), 3f);
            arrow.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(0f, -4f);
            promptRoot = UiKit.Rect(root, "Tuş ipucu", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(520f, 58f));
            var bg = UiKit.Panel(promptRoot, new Color32(51, 37, 28, 225));
            UiKit.Border(bg, new Color32(244, 182, 64, 255), 3f);
            prompt = UiKit.Label(promptRoot, "Yazı", "", 28, new Color32(255, 243, 218, 255), TextAnchor.MiddleCenter, FontStyle.Bold);
            prompt.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetVisible(false);
        }

        public void SetVisible(bool on)
        {
            if (group != null) group.alpha = on ? 1f : 0f;
        }

        /// <summary>target null ise ok gizlenir; promptText boş değilse (yakındayken) tuş ipucu görünür.</summary>
        public void Show(Vector3? target, string promptText)
        {
            if (cam == null) cam = Camera.main;
            if (target == null || cam == null)
            {
                SetVisible(false);
                return;
            }
            SetVisible(true);
            var scale = canvasRect.lossyScale.x > 0f ? canvasRect.lossyScale.x : 1f;
            var sp = cam.WorldToScreenPoint(target.Value + worldOffset);
            var behind = sp.z < 0f;
            var size = canvasRect.rect.size;
            var pos = new Vector2(sp.x, sp.y) / scale;
            if (behind) pos = size - pos;
            var inside = !behind && pos.x > edgeMargin && pos.x < size.x - edgeMargin && pos.y > edgeMargin && pos.y < size.y - edgeMargin;
            var bob = GameSettings.ReduceMotion ? 0f : Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4f)) * bobPixels;
            if (inside)
            {
                arrow.text = "▼";
                arrowRoot.localEulerAngles = Vector3.zero;
                arrowRoot.anchoredPosition = pos + new Vector2(0f, bob);
            }
            else
            {
                // Ekran kenarına sabitle, hedef yönüne döndür.
                var center = size * 0.5f;
                var dir = (pos - center).normalized;
                var clamped = new Vector2(Mathf.Clamp(pos.x, edgeMargin, size.x - edgeMargin), Mathf.Clamp(pos.y, edgeMargin, size.y - edgeMargin));
                arrow.text = "▼";
                arrowRoot.anchoredPosition = clamped - dir * bob;
                arrowRoot.localEulerAngles = new Vector3(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f);
            }
            var hasPrompt = !string.IsNullOrEmpty(promptText);
            promptRoot.gameObject.SetActive(hasPrompt);
            if (hasPrompt)
            {
                prompt.text = promptText;
                promptRoot.sizeDelta = new Vector2(Mathf.Max(260f, prompt.preferredWidth + 48f), 58f);
                var p = (inside ? pos : arrowRoot.anchoredPosition) + new Vector2(0f, 96f);
                p.x = Mathf.Clamp(p.x, promptRoot.sizeDelta.x * 0.5f + 10f, size.x - promptRoot.sizeDelta.x * 0.5f - 10f);
                p.y = Mathf.Min(p.y, size.y - 70f);
                promptRoot.anchoredPosition = p;
            }
        }
    }
}
