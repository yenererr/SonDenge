using System;
using UnityEngine;

namespace Jam.Missions
{
    /// <summary>
    /// 100 puanlık rapor (şartname §2.4): hazırlık 35, doğru afet davranışı 35, denge 20, eşya koruma 10.
    /// Saf fonksiyonlar; sahneye bağımlı değildir, MCP/EditMode ile test edilebilir.
    /// </summary>
    public static class MissionScoring
    {
        public const float PreparationMax = 35f;
        public const float BehaviourMax = 35f;
        public const float BalanceMax = 20f;
        public const float ProtectionMax = 10f;
        public const int TwoStarScore = 70;
        public const int ThreeStarScore = 90;

        public static float PreparationPoints(LevelDefinition level, Func<string, bool> isCompleted)
        {
            if (level == null || level.tasks == null || level.tasks.Length == 0) return 0f;
            float total = 0f, done = 0f;
            foreach (var task in level.tasks)
            {
                if (task == null) continue;
                var w = Mathf.Max(0f, task.scoreWeight);
                total += w;
                if (isCompleted(task.id)) done += w;
            }
            return total <= 0f ? 0f : PreparationMax * done / total;
        }

        /// <summary>En yüksek eğime göre denge puanı: 7° altı tam, 26°'de sıfır.</summary>
        public static float BalancePoints(float maxTiltDegrees, float safeAngle = 7f, float collapseAngle = 26f)
        {
            if (maxTiltDegrees <= safeAngle) return BalanceMax;
            var t = Mathf.InverseLerp(safeAngle, collapseAngle, maxTiltDegrees);
            return Mathf.Round(BalanceMax * (1f - t));
        }

        public static int Total(float preparation, float behaviour, float balance, float protection)
        {
            var sum = Mathf.Clamp(preparation, 0f, PreparationMax) + Mathf.Clamp(behaviour, 0f, BehaviourMax) +
                      Mathf.Clamp(balance, 0f, BalanceMax) + Mathf.Clamp(protection, 0f, ProtectionMax);
            return Mathf.Clamp(Mathf.RoundToInt(sum), 0, 100);
        }

        /// <summary>0 = devrildi/başarısız, 1 = güvenli bitti, 2 = ≥70, 3 = ≥90 ve kritik koşulların tamamı.</summary>
        public static int Stars(bool survived, int score, bool allCriticalMet)
        {
            if (!survived) return 0;
            if (score >= ThreeStarScore && allCriticalMet) return 3;
            if (score >= TwoStarScore) return 2;
            return 1;
        }
    }
}
