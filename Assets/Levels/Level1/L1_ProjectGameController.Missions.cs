using System.Collections.Generic;
using Jam.Missions;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Wilson görev modu: mağaza/altın yerine bölüm verisinden (LevelDefinition) gelen görevler,
    /// briefing kapısı, 26° kuralının tüm bölümlerde uygulanması ve 100 puanlık yıldızlı rapor.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        [Header("Görev modu (Wilson, 5 bölüm)")]
        [Tooltip("Açıkken mağaza ve altın yoktur; hazırlık görevlerle yapılır.")]
        public bool missionMode = true;
        public LevelDefinition levelDefinition;
        public MissionManager missions;

        private bool briefingOpen;
        private float behaviourSafeSeconds, behaviourTotalSeconds;
        private bool evacuated;
        private MissionResult lastResult;

        public sealed class MissionResult
        {
            public bool survived;
            public string failReason;
            public int score, stars;
            public float preparation, behaviour, balance, protection;
            public float maxTilt;
            public int keptItems, totalItems;
            public bool allCritical;
            public readonly List<string> lines = new List<string>();
        }

        public bool IsBriefingOpen => briefingOpen;
        public bool BalanceReleased => !levelEntryBalanceLocked;
        public MissionResult LastResult => lastResult;
        public bool IsVictory => state == RoundState.Victory;
        public LevelDisaster Disaster => levelDefinition != null ? levelDefinition.disaster : (LevelDisaster)Mathf.Clamp(level - 1, 0, 4);
        public event System.Action DisasterStarted;
        public event System.Action<MissionResult> LevelFinished;

        private float PreparationSeconds => missionMode && levelDefinition != null ? levelDefinition.preparationSeconds : 45f;

        private float DisasterSeconds => missionMode && levelDefinition != null ? levelDefinition.disasterSeconds :
            level == 1 ? quakeDuration : level == 2 ? stormDuration : level == 3 ? fireMaxTime :
            Mathf.Max(lightningDuration, rainDuration);

        /// <summary>Briefing açıkken hazırlık saati, çökme sayacı ve Wilson'ın yumurtlaması durur.</summary>
        public void SetBriefingOpen(bool open)
        {
            briefingOpen = open;
            if (open && drag != null) drag.DropCarried(true);
            SyncPause();
        }

        /// <summary>Tamamlanan görevin afet simülasyonundaki koruma anahtarını açar.</summary>
        public void GrantProtection(string key)
        {
            if (!string.IsNullOrEmpty(key)) equipment.Add(key);
        }

        public bool HasProtection(string key) => equipment.Contains(key);

        /// <summary>Afet sırasında "güvenli davranış" süresini biriktirir (bölüme göre yönetmen hesaplar).</summary>
        public void AccumulateBehaviour(bool safe, float dt)
        {
            if (state != RoundState.Disaster) return;
            behaviourTotalSeconds += dt;
            if (safe) behaviourSafeSeconds += dt;
        }

        private void ApplyLevelSetup()
        {
            if (!missionMode || levelDefinition == null) return;
            level = Mathf.Clamp(levelDefinition.levelNumber, 1, 5);
            growDownward = false;
            singleFloorLevel = false;
            if (levelDefinition.floorCount >= 3)
            {
                SetUpperFloorVisible(true);
                RefreshFurniture();
            }
            // Şiddetli yağmurda pompa önceden kurulu, otomatik ve güvenli bir sistemdir; oyuncu
            // ıslak ortamda elektrikli pompaya dokunmaz (şartname §5 / §11).
            if (level == 5) equipment.Add("water_pump");
            if (missions == null) missions = GetComponent<MissionManager>();
            if (missions == null) missions = gameObject.AddComponent<MissionManager>();
            if (missions.Level != levelDefinition) missions.SetLevel(levelDefinition);
            else missions.ResetTasks();
        }

        private void BeginMissionDisaster()
        {
            behaviourSafeSeconds = behaviourTotalSeconds = 0f;
            evacuated = false;
            if (missions != null) missions.LockForDisaster();
            BeginQuakeRecord();
            DisasterStarted?.Invoke();
        }

        /// <summary>Yangında güvenli tahliye: kaçış yolu açıksa başarı sayılır.</summary>
        private void Evacuate()
        {
            evacuated = true;
            CompleteMissionLevel(true, null);
        }

        private void CompleteMissionLevel(bool survived, string reason)
        {
            if (state == RoundState.Report || state == RoundState.GameOver || state == RoundState.Victory) return;
            var wasDisaster = state == RoundState.Disaster;
            TrackLostFurniture();
            var r = new MissionResult { survived = survived, failReason = reason, maxTilt = maxTilt };
            r.totalItems = quakeRecords.Count;
            foreach (var record in quakeRecords) if (!record.lost) r.keptItems++;
            r.preparation = missions != null ? missions.PreparationPoints : 0f;
            r.allCritical = missions != null && missions.AllCriticalCompleted;
            r.behaviour = !wasDisaster ? 0f : BehaviourPoints();
            r.balance = survived ? MissionScoring.BalancePoints(maxTilt, 7f, collapseAngle) : 0f;
            r.protection = r.totalItems == 0 ? MissionScoring.ProtectionMax :
                MissionScoring.ProtectionMax * r.keptItems / r.totalItems;
            r.score = survived ? MissionScoring.Total(r.preparation, r.behaviour, r.balance, r.protection) : 0;
            r.stars = MissionScoring.Stars(survived, r.score, r.allCritical);
            BuildResultLines(r);
            lastResult = r;
            if (levelDefinition != null) LevelProgressSave.Record(levelDefinition.levelNumber, r.stars);
            if (drag != null) drag.DropCarried(true);
            SetMusic(planningMusic);
            SetState(survived ? RoundState.Report : RoundState.GameOver);
            if (sfx != null) sfx.Play(survived ? "level" : "defeat");
            LevelFinished?.Invoke(r);
        }

        private float BehaviourPoints()
        {
            var ratio = behaviourTotalSeconds <= 0.01f ? 0f : behaviourSafeSeconds / behaviourTotalSeconds;
            if (level == 3)
            {
                // Yangın: söndürülen küçük yangın ya da açık yoldan tahliye doğru davranıştır.
                if (fires.Count == 0) return MissionScoring.BehaviourMax;
                if (evacuated) return MissionScoring.BehaviourMax * 0.85f;
                return 0f;
            }
            return Mathf.Round(MissionScoring.BehaviourMax * Mathf.Clamp01(ratio / 0.8f));
        }

        private void BuildResultLines(MissionResult r)
        {
            if (levelDefinition == null) return;
            foreach (var task in levelDefinition.tasks)
            {
                if (task == null) continue;
                var done = missions != null && missions.IsCompleted(task.id);
                // Rapor yalnız gerçekleşeni söyler: sabitlenen eşya kaybolduysa "korundu" yazılmaz.
                if (done && TaskEffectHeld(task)) r.lines.Add("● " + task.reportTr);
                else if (done) r.lines.Add("● " + task.titleTr + " tamamlandı, ancak afette yeterli olmadı.");
                else r.lines.Add("× " + task.titleTr + " yapılmadı.");
            }
            if (level == 1)
                r.lines.Add(r.behaviour >= MissionScoring.BehaviourMax * 0.5f
                    ? "● Deprem sırasında ÇÖK–KAPAN–TUTUN uyguladın."
                    : "× Deprem sırasında ÇÖK–KAPAN–TUTUN yeterince uygulanmadı (BOŞLUK).");
            else if (level == 3)
                r.lines.Add(fires.Count == 0 ? "● Küçük yangın güvenli biçimde söndürüldü." :
                    evacuated ? "● Yangın büyüyünce açık kaçış yolundan güvenle tahliye ettin." :
                    "× Yangın kontrolden çıktı ve güvenli tahliye yapılamadı.");
            else if (level == 2 || level == 4)
                r.lines.Add(r.behaviour >= MissionScoring.BehaviourMax * 0.5f
                    ? "● Afet sırasında pencerelerden uzak durdun."
                    : "× Afet sırasında pencere/elektrik yakınında çok kaldın.");
            else if (level == 5)
                r.lines.Add(r.behaviour >= MissionScoring.BehaviourMax * 0.5f
                    ? "● Su yükselirken yüksek ve güvenli katta kaldın."
                    : "× Su yükselirken alt katta, elektriğe yakın kaldın.");
            r.lines.Add("Kaybolan eşya: " + (r.totalItems - r.keptItems) + " / " + r.totalItems +
                        "   ·   En yüksek eğim: " + r.maxTilt.ToString("F1") + "°");
        }

        private bool TaskEffectHeld(TaskDefinition task)
        {
            switch (task.protectionKey)
            {
                case "anchor_bookshelf": return !LostItemNamed("kitaplık");
                case "anchor_fridge": return !LostItemNamed("buzdolabı");
                case "duman_dedektoru": return state != RoundState.Planning;
                case "lightning_rod_system": return lightningBlocked > 0 || lightningHits == 0;
                case "sandbags": return indoorFlood < 100f;
                default: return true;
            }
        }

        private bool LostItemNamed(string key)
        {
            foreach (var record in quakeRecords)
                if (record.lost && record.name != null && record.name.ToLowerInvariant().Contains(key)) return true;
            return false;
        }

        private void GoToNextMissionScene()
        {
            if (levelDefinition == null || string.IsNullOrEmpty(levelDefinition.nextSceneName)) return;
            Time.timeScale = 1f;
            if (Jam.Core.SceneLoader.Instance != null) Jam.Core.SceneLoader.Instance.Load(levelDefinition.nextSceneName);
        }

        public bool HasNextScene => levelDefinition != null && !string.IsNullOrEmpty(levelDefinition.nextSceneName);

        public void UiEvacuate()
        {
            if (state != RoundState.Disaster || level != 3) return;
            if (!equipment.Contains("exit_clear")) { Warn("Kaçış yolu kapalı! Merdiven önü dolu."); return; }
            Evacuate();
        }

        public void UiLoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            Time.timeScale = 1f;
            if (Jam.Core.SceneLoader.Instance != null) Jam.Core.SceneLoader.Instance.Load(sceneName);
        }
    }
}
