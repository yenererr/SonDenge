using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Sabit yan kamera: fare tekeriyle FOV yakınlaştırma, orta fareyle sınırlı kaydırma,
    /// Home / Sığdır ile başlangıç kadrajı. Kamera hiçbir zaman dönmez.
    /// Sarsıntı ayrı bir ofset olarak eklenir; zoom/pan hedefini bozmaz.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class L1_CameraController : MonoBehaviour
    {
        [Header("Kadraj")]
        [SerializeField] private Vector3 restPosition = new Vector3(0f, -0.4f, -14f);
        [SerializeField] private float restFov = 40f;
        [SerializeField] private float minFov = 27f;
        [SerializeField] private float maxFov = 52f;
        [Header("Zoom")]
        [SerializeField] private float wheelStep = 3f;
        [SerializeField] private float buttonStep = 4f;
        [SerializeField] private float smoothTime = 0.14f;
        [Header("Kaydırma sınırları (dünya birimi)")]
        [SerializeField] private Vector2 panMin = new Vector2(-4f, -3f);
        [SerializeField] private Vector2 panMax = new Vector2(4f, 1.5f);
        [Header("Durum")]
        public bool inputEnabled = true;

        private Camera cam;
        private float targetFov, fovVelocity;
        private Vector2 targetPan, pan, panVelocity;
        private bool panning;
        private Vector2 panStartMouse, panStartValue;
        private float shakeUntil, shakeDuration, shakeStrength;
        private Vector3 shakeOffset;
        private Quaternion restRotation;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = false;
            restRotation = transform.rotation;
            Fit(true);
        }

        public void ZoomIn() => targetFov = Mathf.Clamp(targetFov - buttonStep, minFov, maxFov);
        public void ZoomOut() => targetFov = Mathf.Clamp(targetFov + buttonStep, minFov, maxFov);

        public void Fit() => Fit(false);

        private void Fit(bool instant)
        {
            targetFov = restFov;
            targetPan = Vector2.zero;
            if (!instant) return;
            pan = targetPan;
            cam.fieldOfView = targetFov;
            fovVelocity = 0f;
            panVelocity = Vector2.zero;
        }

        /// <summary>Ev büyüyünce başlangıç kadrajını kaydırır ve biraz uzaklaştırır.</summary>
        public void ShiftRest(float dy, float fovIncrease)
        {
            restPosition += new Vector3(0f, dy, 0f);
            restFov = Mathf.Min(maxFov, restFov + fovIncrease);
            maxFov = Mathf.Max(maxFov, restFov + 8f);
            panMin.y += dy;
            panMax.y += dy;
            targetFov = restFov;
            targetPan = Vector2.zero;
        }

        /// <summary>Kameraya kısa sarsıntı ekler (gerçek zoom/pan konumunu değiştirmez).</summary>
        public void Shake(float duration, float strength)
        {
            shakeDuration = Mathf.Max(0.01f, duration);
            shakeUntil = Time.unscaledTime + duration;
            shakeStrength = strength;
        }

        private void Update()
        {
            if (!inputEnabled || Time.timeScale <= 0f) { panning = false; return; }
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var overUi = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (keyboard != null && keyboard.homeKey.wasPressedThisFrame) Fit();
            if (mouse == null) return;

            var scroll = mouse.scroll.ReadValue().y;
            if (!overUi && Mathf.Abs(scroll) > 0.01f)
                targetFov = Mathf.Clamp(targetFov - Mathf.Sign(scroll) * wheelStep, minFov, maxFov);

            if (mouse.middleButton.wasPressedThisFrame && !overUi)
            {
                panning = true;
                panStartMouse = mouse.position.ReadValue();
                panStartValue = targetPan;
            }
            if (!mouse.middleButton.isPressed) panning = false;
            if (panning)
            {
                // Ekran pikselini kamera mesafesindeki dünya birimine çevir.
                var distance = Mathf.Abs(restPosition.z);
                var worldPerPixel = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
                var delta = (mouse.position.ReadValue() - panStartMouse) * worldPerPixel;
                targetPan = ClampPan(panStartValue - delta);
            }
        }

        private Vector2 ClampPan(Vector2 value) =>
            new Vector2(Mathf.Clamp(value.x, panMin.x, panMax.x), Mathf.Clamp(value.y, panMin.y, panMax.y));

        private void LateUpdate()
        {
            var dt = Time.unscaledDeltaTime;
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovVelocity, smoothTime, Mathf.Infinity, dt);
            pan = Vector2.SmoothDamp(pan, ClampPan(targetPan), ref panVelocity, smoothTime, Mathf.Infinity, dt);

            shakeOffset = Vector3.zero;
            if (Time.unscaledTime < shakeUntil)
            {
                var damper = (shakeUntil - Time.unscaledTime) / shakeDuration;
                shakeOffset = (Vector3)(Random.insideUnitCircle * shakeStrength * damper);
            }
            transform.SetPositionAndRotation(restPosition + new Vector3(pan.x, pan.y, 0f) + shakeOffset,
                restRotation);
        }
    }
}
