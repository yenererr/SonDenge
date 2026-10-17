using Jam.Core;
using UnityEngine;

namespace Jam.UI
{
    /// <summary>
    /// Oyuncu ayarları (PlayerPrefs). Menüdeki AYARLAR paneli yazar, bölümler okur.
    /// </summary>
    public static class GameSettings
    {
        private const string MasterKey = "son_denge_ses_ana";
        private const string MusicKey = "son_denge_ses_muzik";
        private const string SfxKey = "son_denge_ses_efekt";
        private const string ReduceShakeKey = "son_denge_sarsinti_azalt";

        public static float Master
        {
            get => PlayerPrefs.GetFloat(MasterKey, 1f);
            set { PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value)); Apply(); }
        }

        public static float Music
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.7f);
            set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); Apply(); }
        }

        public static float Sfx
        {
            get => PlayerPrefs.GetFloat(SfxKey, 1f);
            set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); Apply(); }
        }

        /// <summary>Açıksa kamera sarsıntısı çok hafifler (görsel hassasiyet için).</summary>
        public static bool ReduceShake
        {
            get => PlayerPrefs.GetInt(ReduceShakeKey, 0) == 1;
            set => PlayerPrefs.SetInt(ReduceShakeKey, value ? 1 : 0);
        }

        public static void ResetToDefaults()
        {
            PlayerPrefs.DeleteKey(MasterKey);
            PlayerPrefs.DeleteKey(MusicKey);
            PlayerPrefs.DeleteKey(SfxKey);
            PlayerPrefs.DeleteKey(ReduceShakeKey);
            Apply();
        }

        public static void Apply()
        {
            AudioListener.volume = Master;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.musicVolume = Music;
                AudioManager.Instance.sfxVolume = Sfx;
            }
        }
    }
}
