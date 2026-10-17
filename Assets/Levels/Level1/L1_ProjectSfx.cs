using System.Collections.Generic;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Short synthesized cues adapted from project-files' Web Audio gameAudio tones.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class L1_ProjectSfx : MonoBehaviour
    {
        private const int Rate = 22050;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource source;
        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = 0.23f;
        }

        public void Play(string cue)
        {
            if (source == null) return;
            if (!clips.TryGetValue(cue, out var clip))
            {
                clip = Build(cue);
                clips.Add(cue, clip);
            }
            source.PlayOneShot(clip);
        }

        private static AudioClip Build(string cue)
        {
            var notes = cue == "purchase" ? new[] { 523f, 659f, 784f, 1046f } :
                cue == "level" ? new[] { 262f, 330f, 392f, 523f, 659f, 784f } :
                cue == "victory" ? new[] { 262f, 330f, 392f, 523f, 784f } :
                cue == "defeat" ? new[] { 311f, 262f, 247f, 196f } :
                cue == "coin" ? new[] { 988f, 1319f } :
                cue == "error" ? new[] { 150f, 159f } :
                cue == "strike" ? new[] { 120f, 40f } :
                cue == "shield" ? new[] { 100f, 40f } : new[] { 523f, 784f };
            var interval = cue == "level" ? 0.07f : cue == "victory" ? 0.10f :
                cue == "defeat" ? 0.15f : cue == "error" ? 0f : 0.08f;
            var toneLength = cue == "defeat" ? 0.32f : cue == "strike" ? 0.25f : 0.20f;
            var duration = Mathf.Max(0.25f, (notes.Length - 1) * interval + toneLength + 0.04f);
            var samples = new float[Mathf.CeilToInt(duration * Rate)];
            for (var n = 0; n < notes.Length; n++)
            {
                var start = Mathf.RoundToInt(n * interval * Rate);
                var count = Mathf.RoundToInt(toneLength * Rate);
                for (var i = 0; i < count && start + i < samples.Length; i++)
                {
                    var t = i / (float)Rate;
                    var envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / toneLength));
                    var frequency = notes[n];
                    var phase = 2f * Mathf.PI * frequency * t;
                    var wave = cue == "error" || cue == "defeat" ? 2f * Mathf.Repeat(phase / (2f * Mathf.PI), 1f) - 1f :
                        Mathf.Sin(phase);
                    samples[start + i] += wave * envelope * (cue == "strike" ? 0.55f : 0.35f);
                }
            }
            if (cue == "strike" || cue == "shield")
                for (var i = 0; i < samples.Length; i++)
                {
                    var t = i / (float)Rate;
                    var noise = Mathf.Sin(i * 78.233f) * Mathf.Sin(i * 17.719f);
                    samples[i] += noise * Mathf.Exp(-t * 19f) * (cue == "strike" ? 0.27f : 0.12f);
                }
            var clip = AudioClip.Create("L1 " + cue, samples.Length, 1, Rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
