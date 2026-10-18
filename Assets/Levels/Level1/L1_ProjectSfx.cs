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

        private const string ClipFolder = "Sfx/sfx_";
        private readonly Dictionary<string, AudioClip[]> recorded = new Dictionary<string, AudioClip[]>();
        private readonly Dictionary<string, AudioSource> loops = new Dictionary<string, AudioSource>();
        private AudioSource clipSource;

        private static float SfxVolume => Jam.Core.AudioManager.Instance != null ? Jam.Core.AudioManager.Instance.sfxVolume : 1f;

        /// <summary>
        /// Kayıtlı ses (Resources/Sfx/sfx_cue.wav ya da sfx_cue_1..n varyasyonları) varsa onu çalar.
        /// Yoksa false döner; Play() bu durumda sentez sese düşer.
        /// </summary>
        public bool PlayClip(string cue, float volume = 1f, float pitchJitter = 0.05f)
        {
            var variants = Recorded(cue);
            if (variants.Length == 0) return false;
            if (clipSource == null)
            {
                clipSource = gameObject.AddComponent<AudioSource>();
                clipSource.playOnAwake = false;
                clipSource.spatialBlend = 0f;
            }
            clipSource.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            clipSource.PlayOneShot(variants[Random.Range(0, variants.Length)], volume * SfxVolume);
            return true;
        }

        /// <summary>Döngü sesini (sfx_cue_loop) yumuşakça açar/kapatır. Her karede çağrılabilir.</summary>
        public void SetLoop(string cue, bool on, float volume = 0.6f, float fadePerSecond = 1.5f)
        {
            if (!loops.TryGetValue(cue, out var loop))
            {
                if (!on) return;
                var clip = Resources.Load<AudioClip>(ClipFolder + cue + "_loop");
                if (clip == null) return;
                loop = gameObject.AddComponent<AudioSource>();
                loop.clip = clip;
                loop.loop = true;
                loop.playOnAwake = false;
                loop.volume = 0f;
                loops.Add(cue, loop);
            }
            var target = on ? volume * SfxVolume : 0f;
            loop.volume = Mathf.MoveTowards(loop.volume, target, fadePerSecond * Time.unscaledDeltaTime);
            if (loop.volume > 0f && !loop.isPlaying) loop.Play();
            else if (loop.volume <= 0f && loop.isPlaying) loop.Stop();
        }

        /// <summary>Tüm döngüleri hemen susturur (menü, rapor, bölüm sonu).</summary>
        public void StopLoops()
        {
            foreach (var loop in loops.Values) { loop.volume = 0f; loop.Stop(); }
        }

        private AudioClip[] Recorded(string cue)
        {
            if (recorded.TryGetValue(cue, out var found)) return found;
            var list = new List<AudioClip>();
            var single = Resources.Load<AudioClip>(ClipFolder + cue);
            if (single != null) list.Add(single);
            for (var i = 1; i < 10; i++)
            {
                var variant = Resources.Load<AudioClip>(ClipFolder + cue + "_" + i);
                if (variant == null) break;
                list.Add(variant);
            }
            found = list.ToArray();
            recorded.Add(cue, found);
            return found;
        }

        public void Play(string cue)
        {
            if (source == null) return;
            if (PlayClip(cue)) return;
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
