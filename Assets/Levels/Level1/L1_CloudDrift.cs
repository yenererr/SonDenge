using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Moves scenery clouds slowly without affecting the house or physics.</summary>
    public sealed class L1_CloudDrift : MonoBehaviour
    {
        [SerializeField] private float horizontalRange = 1f;
        [SerializeField] private float cycleSeconds = 40f;
        [SerializeField] private float phase;

        private Vector3 startPosition;
        private float startTime;

        public void Configure(float range, float seconds, float startPhase)
        {
            horizontalRange = Mathf.Max(0f, range);
            cycleSeconds = Mathf.Max(1f, seconds);
            phase = startPhase;
        }

        private void Start()
        {
            startPosition = transform.localPosition;
            startTime = Time.unscaledTime;
        }

        private void LateUpdate()
        {
            var angle = (Time.unscaledTime - startTime) * (Mathf.PI * 2f / cycleSeconds);
            var horizontal = (Mathf.Sin(angle + phase) - Mathf.Sin(phase)) * horizontalRange;
            var vertical = (Mathf.Sin(angle * 0.53f + phase) - Mathf.Sin(phase)) * 0.06f;
            transform.localPosition = startPosition + new Vector3(horizontal, vertical, 0f);
        }
    }
}
