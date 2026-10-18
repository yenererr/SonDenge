using Jam.Core;
using UnityEngine;

namespace Jam.UI
{
    /// <summary>
    /// Oyuncu ayarları (PlayerPrefs). Menüdeki AYARLAR ekranı yazar, bölümler okur.
    /// Eski anahtar adları korunur; yeni anahtarlar aynı "son_denge_" önekini kullanır.
    /// </summary>
    public static class GameSettings
    {
        private const string MasterKey = "son_denge_ses_ana";
        private const string MusicKey = "son_denge_ses_muzik";
        private const string SfxKey = "son_denge_ses_efekt";
        private const string MuteKey = "son_denge_ses_kapali";
        private const string ReduceShakeKey = "son_denge_sarsinti_azalt";
        private const string ReduceMotionKey = "son_denge_hareket_azalt";
        private const string VSyncKey = "son_denge_vsync";
        private const string QualityKey = "son_denge_kalite";

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

        /// <summary>Bütün sesi kapatır (seviyeler korunur).</summary>
        public static bool Muted
        {
            get => PlayerPrefs.GetInt(MuteKey, 0) == 1;
            set { PlayerPrefs.SetInt(MuteKey, value ? 1 : 0); Apply(); }
        }

        /// <summary>Açıksa kamera sarsıntısı çok hafifler (görsel hassasiyet için).</summary>
        public static bool ReduceShake
        {
            get => PlayerPrefs.GetInt(ReduceShakeKey, 0) == 1;
            set => PlayerPrefs.SetInt(ReduceShakeKey, value ? 1 : 0);
        }

        /// <summary>Açıksa menü geçişleri kısalır, arka plan kayması ve Wilson zıplaması durur.</summary>
        public static bool ReduceMotion
        {
            get => PlayerPrefs.GetInt(ReduceMotionKey, 0) == 1;
            set => PlayerPrefs.SetInt(ReduceMotionKey, value ? 1 : 0);
        }

        public static bool VSync
        {
            get => PlayerPrefs.GetInt(VSyncKey, 1) == 1;
            set { PlayerPrefs.SetInt(VSyncKey, value ? 1 : 0); ApplyQuality(); }
        }

        /// <summary>QualitySettings.names içindeki seviye; kayıt yoksa projenin varsayılanı.</summary>
        public static int Quality
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(QualityKey, QualitySettings.GetQualityLevel()), 0, QualitySettings.names.Length - 1);
            set { PlayerPrefs.SetInt(QualityKey, value); ApplyQuality(); }
        }

        public static void ResetToDefaults()
        {
            foreach (var key in new[] { MasterKey, MusicKey, SfxKey, MuteKey, ReduceShakeKey, ReduceMotionKey, VSyncKey, QualityKey })
                PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Apply();
            ApplyQuality();
        }

        public static void Save() => PlayerPrefs.Save();

        public static void Apply()
        {
            AudioListener.volume = Muted ? 0f : Master;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.musicVolume = Music;
                AudioManager.Instance.sfxVolume = Sfx;
                AudioManager.Instance.RefreshVolumes();
            }
        }

        public static void ApplyQuality()
        {
            if (PlayerPrefs.HasKey(QualityKey) && QualitySettings.GetQualityLevel() != Quality)
                QualitySettings.SetQualityLevel(Quality, true);
            QualitySettings.vSyncCount = VSync ? 1 : 0;
        }
    }
}
