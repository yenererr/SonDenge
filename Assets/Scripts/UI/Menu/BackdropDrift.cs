using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Arka plan görüntüsünü çok yavaş kaydırır (canlı his). Hareketleri azalt açıkken durur.</summary>
    [RequireComponent(typeof(RawImage))]
    public sealed class BackdropDrift : MonoBehaviour
    {
        [SerializeField] private Rect baseRect = new Rect(0.02f, 0.02f, 0.96f, 0.96f);
        [SerializeField] private Vector2 amplitude = new Vector2(0.015f, 0.008f);
        [SerializeField] private float period = 40f;

        private RawImage image;

        private void Awake() => image = GetComponent<RawImage>();

        private void Update()
        {
            var t = GameSettings.ReduceMotion ? 0f : Time.unscaledTime * Mathf.PI * 2f / Mathf.Max(1f, period);
            image.uvRect = new Rect(baseRect.x + Mathf.Sin(t) * amplitude.x, baseRect.y + Mathf.Sin(t * 0.7f) * amplitude.y,
                baseRect.width, baseRect.height);
        }
    }
}
