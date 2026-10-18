using Jam.Core;
using UnityEngine;

namespace Jam.UI.Menu
{
    public enum MenuSound { Click, Hover, Open, Confirm, Error }

    /// <summary>
    /// Menü seslerini mevcut AudioManager üzerinden çalar (ikinci AudioManager kurmaz).
    /// Düğmeler bu bileşeni üst hiyerarşiden bulur.
    /// </summary>
    public sealed class MenuAudioBridge : MonoBehaviour
    {
        [SerializeField] private AudioClip click, hover, open, confirm, error;
        [SerializeField, Range(0f, 1f)] private float volume = 0.7f;
        [SerializeField, Range(0f, 1f)] private float hoverVolume = 0.35f;

        public void Play(MenuSound sound)
        {
            if (AudioManager.Instance == null) return;
            switch (sound)
            {
                case MenuSound.Click: AudioManager.Instance.PlaySfx(click, volume, 0.04f); break;
                case MenuSound.Hover: AudioManager.Instance.PlaySfx(hover, hoverVolume, 0.06f); break;
                case MenuSound.Open: AudioManager.Instance.PlaySfx(open, volume * 0.8f, 0.02f); break;
                case MenuSound.Confirm: AudioManager.Instance.PlaySfx(confirm, volume, 0f); break;
                case MenuSound.Error: AudioManager.Instance.PlaySfx(error, volume, 0f); break;
            }
        }
    }
}
