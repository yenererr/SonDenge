using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Duvar meşalesi ışığını hafifçe titretir (Perlin gürültüsü; her meşale farklı fazda).</summary>
    [RequireComponent(typeof(Light))]
    public sealed class L1_TorchFlicker : MonoBehaviour
    {
        [SerializeField] private float baseIntensity = 1.25f;
        [SerializeField, Range(0f, 0.6f)] private float flickerAmount = 0.22f;
        [SerializeField] private float speed = 6f;

        private Light torchLight;
        private float seed;

        private void Awake()
        {
            torchLight = GetComponent<Light>();
            seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            var noise = Mathf.PerlinNoise(seed, Time.time * speed) - 0.5f;
            torchLight.intensity = baseIntensity * (1f + noise * 2f * flickerAmount);
        }
    }
}
