using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Menüdeki tek bir ekran. Açık olan tek sayfa input alır; kapanan sayfa raycast'i hemen bırakır.
    /// Geçiş: CanvasGroup alpha + hafif ölçek, unscaled zamanla (Time.timeScale = 0 iken de çalışır).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuPage : MonoBehaviour
    {
        [SerializeField] protected Selectable firstSelected;
        [SerializeField, Range(0.05f, 0.6f)] private float duration = 0.24f;
        [Tooltip("Ölçek animasyonu uygulanan iç kök (boşsa sayfanın kendisi).")]
        [SerializeField] private RectTransform animatedRoot;
        [Tooltip("Sırayla soldan kayarak beliren öğeler (ör. ana menü düğmeleri).")]
        [SerializeField] private RectTransform[] staggered = new RectTransform[0];
        [SerializeField] private float staggerDelay = 0.06f;

        protected MenuFlowController Flow { get; private set; }
        public bool IsOpen { get; private set; }

        private CanvasGroup group;
        private Coroutine anim;

        public void Bind(MenuFlowController flow) => Flow = flow;

        protected virtual void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            gameObject.SetActive(true);
            IsOpen = true;
            transform.SetAsLastSibling();
            group.interactable = true;
            group.blocksRaycasts = true;
            OnShown();
            Focus();
            Restart(Animate(true));
        }

        public void Hide(bool instant = false)
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            var wasOpen = IsOpen;
            IsOpen = false;
            group.interactable = false;
            group.blocksRaycasts = false;
            if (wasOpen) OnHidden();
            if (instant || !gameObject.activeInHierarchy)
            {
                group.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }
            Restart(Animate(false));
        }

        /// <summary>Sayfa açıldığında verileri tazelemek için.</summary>
        protected virtual void OnShown() { }

        protected virtual void OnHidden() { }

        /// <summary>ESC / GERİ. true dönerse sayfa olayı kendisi tüketti (ör. açık alt panel kapandı).</summary>
        public virtual bool HandleBack() => false;

        public void Focus()
        {
            if (firstSelected != null && EventSystem.current != null && firstSelected.gameObject.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }

        private void Restart(IEnumerator routine)
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(routine);
        }

        private IEnumerator Animate(bool opening)
        {
            var reduce = GameSettings.ReduceMotion;
            var d = reduce ? duration * 0.35f : duration;
            var root = animatedRoot != null ? animatedRoot : (RectTransform)transform;
            float from = group.alpha, to = opening ? 1f : 0f;
            float s0 = opening ? (reduce ? 1f : 0.965f) : 1f, s1 = opening ? 1f : (reduce ? 1f : 0.985f);
            var groups = opening ? PrepareStagger(reduce) : null;
            for (var t = 0f; t < d + (groups != null ? staggerDelay * staggered.Length : 0f); t += Time.unscaledDeltaTime)
            {
                var k = Mathf.Clamp01(t / d);
                var e = 1f - (1f - k) * (1f - k);
                group.alpha = Mathf.Lerp(from, to, e);
                root.localScale = Vector3.one * Mathf.Lerp(s0, s1, e);
                if (groups != null)
                    for (var i = 0; i < groups.Length; i++)
                    {
                        var ki = Mathf.Clamp01((t - i * staggerDelay) / d);
                        var ei = 1f - (1f - ki) * (1f - ki);
                        groups[i].alpha = ei;
                        staggered[i].anchoredPosition = offsets[i] + new Vector2(Mathf.Lerp(-36f, 0f, ei), 0f);
                    }
                yield return null;
            }
            group.alpha = to;
            root.localScale = Vector3.one;
            if (groups != null)
                for (var i = 0; i < groups.Length; i++) { groups[i].alpha = 1f; staggered[i].anchoredPosition = offsets[i]; }
            if (!opening) gameObject.SetActive(false);
            anim = null;
        }

        private Vector2[] offsets;

        private CanvasGroup[] PrepareStagger(bool reduce)
        {
            if (reduce || staggered == null || staggered.Length == 0) return null;
            if (offsets == null || offsets.Length != staggered.Length)
            {
                offsets = new Vector2[staggered.Length];
                for (var i = 0; i < staggered.Length; i++) offsets[i] = staggered[i].anchoredPosition;
            }
            var result = new CanvasGroup[staggered.Length];
            for (var i = 0; i < staggered.Length; i++)
            {
                var g = staggered[i].GetComponent<CanvasGroup>();
                if (g == null) g = staggered[i].gameObject.AddComponent<CanvasGroup>();
                g.alpha = 0f;
                result[i] = g;
            }
            return result;
        }
    }
}
