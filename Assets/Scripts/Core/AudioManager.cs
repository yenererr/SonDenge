using UnityEngine;

namespace Jam.Core
{
    /// <summary>
    /// Basit ses yöneticisi.
    /// AudioManager.Instance.PlaySfx(clip);  AudioManager.Instance.PlayMusic(clip);
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Range(0f, 1f)] public float musicVolume = 0.6f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private AudioSource _music;
        private AudioSource _sfx;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            _music = gameObject.AddComponent<AudioSource>();
            _music.loop = true;
            _music.playOnAwake = false;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null || _music.clip == clip) return;
            _music.clip = clip;
            _music.volume = musicVolume;
            _music.Play();
        }

        public void StopMusic() => _music.Stop();

        /// <param name="pitchJitter">Tekrarlayan seslere hafif varyasyon (0.1 iyi bir değer).</param>
        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchJitter = 0.05f)
        {
            if (clip == null) return;
            _sfx.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            _sfx.PlayOneShot(clip, volume * sfxVolume);
        }
    }
}
