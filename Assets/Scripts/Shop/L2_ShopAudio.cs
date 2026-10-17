using Jam.Core;
using UnityEngine;

namespace Jam.Shop
{
    /// <summary>
    /// Level2 mağazası ve yerleştirme akışı için küçük, lisans gerektirmeyen sesler.
    /// Sesler kısa sentetik dalga formları olarak oyun başında oluşturulur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class L2_ShopAudio : MonoBehaviour
    {
        [Header("Ses seviyeleri")]
        [Range(0f, 1f)] [SerializeField] private float purchaseVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float placementVolume = 0.72f;
        [Range(0f, 1f)] [SerializeField] private float cancelVolume = 0.62f;

        private const int SampleRate = 44100;
        private AudioClip _purchase;
        private AudioClip _placement;
        private AudioClip _cancel;

        private void Awake()
        {
            _purchase = CreatePurchaseClip();
            _placement = CreatePlacementClip();
            _cancel = CreateCancelClip();
        }

        public void PlayPurchase()
        {
            Play(_purchase, purchaseVolume);
        }

        public void PlayPlacement()
        {
            Play(_placement, placementVolume);
        }

        public void PlayCancel()
        {
            Play(_cancel, cancelVolume);
        }

        private static void Play(AudioClip clip, float volume)
        {
            if (clip != null && AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx(clip, volume, 0.025f);
        }

        private static AudioClip CreatePurchaseClip()
        {
            const float duration = 0.24f;
            return CreateClip("L2_SatinAl", duration, (time, normalized) =>
            {
                float first = Tone(time, 660f, 0.11f, 0.55f);
                float second = Tone(time - 0.075f, 880f, 0.15f, 0.6f);
                return (first + second) * Envelope(normalized, 0.06f, 0.82f);
            });
        }

        private static AudioClip CreatePlacementClip()
        {
            const float duration = 0.22f;
            return CreateClip("L2_Yerlestir", duration, (time, normalized) =>
            {
                float thump = Tone(time, 135f, 0.20f, 0.74f);
                float click = Tone(time, 440f, 0.075f, 0.22f);
                return (thump + click) * Envelope(normalized, 0.015f, 0.66f);
            });
        }

        private static AudioClip CreateCancelClip()
        {
            const float duration = 0.18f;
            return CreateClip("L2_Iptal", duration, (time, normalized) =>
            {
                float frequency = Mathf.Lerp(460f, 220f, normalized);
                return Tone(time, frequency, duration, 0.55f) * Envelope(normalized, 0.015f, 0.72f);
            });
        }

        private delegate float SampleFunction(float time, float normalized);

        private static AudioClip CreateClip(string clipName, float duration, SampleFunction sampleFunction)
        {
            int sampleCount = Mathf.CeilToInt(duration * SampleRate);
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float time = i / (float)SampleRate;
                samples[i] = Mathf.Clamp(sampleFunction(time, i / (float)(sampleCount - 1)), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static float Tone(float time, float frequency, float duration, float amplitude)
        {
            if (time < 0f || time > duration) return 0f;
            float fade = 1f - Mathf.Clamp01(time / duration);
            return Mathf.Sin(time * frequency * Mathf.PI * 2f) * amplitude * fade;
        }

        private static float Envelope(float normalized, float attack, float release)
        {
            float attackPart = Mathf.Clamp01(normalized / attack);
            float releaseStart = 1f - release;
            float releasePart = normalized <= releaseStart
                ? 1f
                : 1f - Mathf.Clamp01((normalized - releaseStart) / release);
            return attackPart * releasePart;
        }

        private void OnDestroy()
        {
            if (_purchase != null) Destroy(_purchase);
            if (_placement != null) Destroy(_placement);
            if (_cancel != null) Destroy(_cancel);
        }
    }
}
