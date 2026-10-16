using System.Collections;
using UnityEngine;

namespace Jam.Core
{
    /// <summary>
    /// Hızlı "juice" yardımcıları: ekran sarsıntısı ve hitstop.
    /// Juice.Shake(0.15f, 0.2f);  Juice.HitStop(0.05f);
    /// </summary>
    public static class Juice
    {
        // Üst üste Shake çağrılarında kamera kaymasın diye tek coroutine + orijinal pozisyon tutulur
        private static Coroutine _shake;
        private static Transform _shakeCam;
        private static Vector3 _shakeOrigin;

        public static void Shake(float duration = 0.15f, float strength = 0.2f)
        {
            var cam = Camera.main;
            if (cam == null || GameManager.Instance == null) return;

            if (_shake != null)
            {
                GameManager.Instance.StopCoroutine(_shake);
                if (_shakeCam != null) _shakeCam.localPosition = _shakeOrigin;
            }
            _shakeCam = cam.transform;
            _shakeOrigin = _shakeCam.localPosition;
            _shake = GameManager.Instance.StartCoroutine(ShakeRoutine(_shakeCam, duration, strength));
        }

        public static void HitStop(float duration = 0.05f)
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.StartCoroutine(HitStopRoutine(duration));
        }

        private static IEnumerator ShakeRoutine(Transform cam, float duration, float strength)
        {
            Vector3 origin = _shakeOrigin;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                if (cam == null) yield break;
                float damper = 1f - t / duration;
                cam.localPosition = origin + (Vector3)Random.insideUnitCircle * strength * damper;
                yield return null;
            }
            if (cam != null) cam.localPosition = origin;
            _shake = null;
        }

        private static IEnumerator HitStopRoutine(float duration)
        {
            if (Time.timeScale == 0f) yield break;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            if (!GameManager.Instance.IsPaused) Time.timeScale = 1f;
        }
    }
}
