using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Bir görevi sahnedeki gerçek nesnelere bağlar ve tamamlanma koşulunu dünya içinden doğrular.
    /// Tamamlanma yalnız L1_MissionDirector üzerinden MissionManager'a bildirilir (tek doğruluk kaynağı).
    /// </summary>
    public sealed class L1_MissionTask : MonoBehaviour
    {
        public enum Condition
        {
            [Tooltip("Tüm 'items' sabitlenmiş olmalı.")] Anchored,
            [Tooltip("Tüm 'items' bırakılmış hâlde 'zone' içinde olmalı.")] ItemsInZone,
            [Tooltip("Tüm 'switches' etkinleştirilmiş olmalı.")] Switches,
            [Tooltip("Ev |eğim| ≤ balanceAngle, balanceSeconds boyunca.")] Balance,
            [Tooltip("'zone' içinde hiçbir 'items' kalmamalı.")] ZoneClear
        }

        [Tooltip("TaskDefinition.id ile aynı, ör. L1_T01.")]
        public string taskId;
        public Condition condition;
        public L1_MovableProp[] items = new L1_MovableProp[0];
        public L1_MissionZone zone;
        public L1_MissionSwitch[] switches = new L1_MissionSwitch[0];
        [Tooltip("Hedef işaretlenecek yer; boşsa 'zone' veya ilk eşya kullanılır.")]
        public Transform hintTarget;
        public float balanceAngle = 7f;
        public float balanceSeconds = 2f;

        private float balanceClock;

        public bool Evaluate(L1_ProjectGameController game, L1_PropDrag drag, float dt)
        {
            switch (condition)
            {
                case Condition.Anchored:
                    if (items.Length == 0) return false;
                    foreach (var item in items) if (item == null || !item.IsAnchored) return false;
                    return true;
                case Condition.ItemsInZone:
                    if (zone == null || items.Length == 0) return false;
                    foreach (var item in items)
                    {
                        if (item == null || !item.gameObject.activeInHierarchy || item.Body == null) return false;
                        if (drag != null && drag.Carried == item) return false;
                        if (!zone.Contains(item.Body.position)) return false;
                    }
                    return true;
                case Condition.Switches:
                    if (switches.Length == 0) return false;
                    foreach (var s in switches) if (s == null || !s.Activated) return false;
                    return true;
                case Condition.Balance:
                    if (game == null) return false;
                    balanceClock = Mathf.Abs(game.TiltDegrees) <= balanceAngle && game.BalanceReleased
                        ? balanceClock + dt : 0f;
                    return balanceClock >= balanceSeconds;
                case Condition.ZoneClear:
                    if (zone == null) return false;
                    foreach (var item in items)
                    {
                        if (item == null || !item.gameObject.activeInHierarchy || item.Body == null) continue;
                        if (drag != null && drag.Carried == item) return false;
                        if (zone.Contains(item.Body.position)) return false;
                    }
                    return true;
            }
            return false;
        }

        public Transform HintTarget =>
            hintTarget != null ? hintTarget :
            zone != null && condition != Condition.ZoneClear ? zone.transform :
            switches.Length > 0 && switches[0] != null ? switches[0].transform :
            items.Length > 0 && items[0] != null ? items[0].transform : transform;

        /// <summary>Görev tamamlanınca hedef işaretini gizler.</summary>
        public void OnCompleted()
        {
            if (zone != null) zone.SetMarkerVisible(false);
        }
    }
}
