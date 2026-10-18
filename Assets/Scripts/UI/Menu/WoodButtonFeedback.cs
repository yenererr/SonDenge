using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Ahşap düğme hissi: üzerine gelince/odakta hafif büyüme ve altın parıltı, tıkta ses.
    /// Etkileşimsiz düğmede tık "hata" sesi verir. Animasyon unscaled çalışır.
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public sealed class WoodButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler,
        IDeselectHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private float hoverScale = 1.04f;
        [SerializeField] private float pressScale = 0.97f;
        [SerializeField] private float speed = 14f;
        [Tooltip("İsteğe bağlı parıltı (alpha ile açılır).")]
        [SerializeField] private Graphic glow;
        [SerializeField, Range(0f, 1f)] private float idleGlow = 0f;
        [SerializeField] private bool playClick = true;

        private Selectable selectable;
        private MenuAudioBridge audioBridge;
        private bool hovered, selected, pressed;
        private float scale = 1f;

        private void Awake()
        {
            selectable = GetComponent<Selectable>();
            audioBridge = GetComponentInParent<MenuAudioBridge>(true);
        }

        private void OnDisable()
        {
            hovered = selected = pressed = false;
            scale = 1f;
            transform.localScale = Vector3.one;
            if (glow != null) SetGlow(idleGlow);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            hovered = true;
            if (selectable.IsInteractable() && audioBridge != null) audioBridge.Play(MenuSound.Hover);
        }

        public void OnPointerExit(PointerEventData e) { hovered = false; pressed = false; }
        public void OnSelect(BaseEventData e) => selected = true;
        public void OnDeselect(BaseEventData e) => selected = false;
        public void OnPointerDown(PointerEventData e) => pressed = true;
        public void OnPointerUp(PointerEventData e) => pressed = false;

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) Feedback();
        }

        public void OnSubmit(BaseEventData e) => Feedback();

        private void Feedback()
        {
            if (audioBridge == null || !playClick) return;
            audioBridge.Play(selectable.IsInteractable() ? MenuSound.Click : MenuSound.Error);
        }

        private void Update()
        {
            var active = selectable.IsInteractable() && (hovered || selected);
            var target = pressed && selectable.IsInteractable() ? pressScale : active ? hoverScale : 1f;
            if (GameSettings.ReduceMotion) target = 1f;
            scale = Mathf.Lerp(scale, target, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime));
            transform.localScale = new Vector3(scale, scale, 1f);
            if (glow != null)
            {
                var a = glow.color.a;
                SetGlow(Mathf.Lerp(a, active ? 1f : idleGlow, 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime)));
            }
        }

        private void SetGlow(float a)
        {
            var c = glow.color;
            c.a = a;
            glow.color = c;
        }
    }
}
