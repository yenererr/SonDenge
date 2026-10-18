using UnityEngine;

namespace Jam.Missions
{
    /// <summary>
    /// Bölüm yıldızları ve kilit durumu (PlayerPrefs, sürümlü anahtar).
    /// Kayıt yoksa ya da bozuksa yalnız Level1 açıktır.
    /// </summary>
    public static class LevelProgressSave
    {
        public const int LevelCount = 5;
        private const string Prefix = "sondenge.v1.";

        public static int BestStars(int levelNumber) =>
            Mathf.Clamp(PlayerPrefs.GetInt(Prefix + "stars.L" + levelNumber, 0), 0, 3);

        public static bool IsUnlocked(int levelNumber)
        {
            if (levelNumber <= 1) return true;
            return PlayerPrefs.GetInt(Prefix + "unlocked.L" + levelNumber, 0) == 1;
        }

        /// <summary>Sonucu kaydeder; daha iyi yıldızı korur, 1+ yıldızda sonraki bölümü açar.</summary>
        public static void Record(int levelNumber, int stars)
        {
            if (levelNumber < 1 || levelNumber > LevelCount) return;
            if (stars > BestStars(levelNumber)) PlayerPrefs.SetInt(Prefix + "stars.L" + levelNumber, Mathf.Clamp(stars, 0, 3));
            if (stars >= 1 && levelNumber < LevelCount) PlayerPrefs.SetInt(Prefix + "unlocked.L" + (levelNumber + 1), 1);
            PlayerPrefs.Save();
        }

        public static bool BriefingSeen(int levelNumber) => PlayerPrefs.GetInt(Prefix + "briefing.L" + levelNumber, 0) == 1;

        public static void MarkBriefingSeen(int levelNumber)
        {
            PlayerPrefs.SetInt(Prefix + "briefing.L" + levelNumber, 1);
            PlayerPrefs.Save();
        }

        public static void ResetAll()
        {
            for (var i = 1; i <= LevelCount; i++)
            {
                PlayerPrefs.DeleteKey(Prefix + "stars.L" + i);
                PlayerPrefs.DeleteKey(Prefix + "unlocked.L" + i);
                PlayerPrefs.DeleteKey(Prefix + "briefing.L" + i);
            }
            PlayerPrefs.Save();
        }
    }
}
