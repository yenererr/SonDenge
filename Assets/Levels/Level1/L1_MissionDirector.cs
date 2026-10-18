using System.Collections;
using Jam.Missions;
using Jam.UI;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Bölümün görev akışını yönetir: Wilson brifingi (süre durur) → hazırlık (görevler dünya
    /// içinden doğrulanır) → afet (davranış ölçülür) → yıldızlı AFET RAPORU. Oyun durumu
    /// L1_ProjectGameController'da kalır; bu bileşen ikinci bir durum makinesi kurmaz.
    /// </summary>
    public sealed class L1_MissionDirector : MonoBehaviour
    {
        [SerializeField] private L1_ProjectGameController game;
        [SerializeField] private L1_PropDrag drag;
        [Tooltip("Boşsa evin altındaki bütün L1_MissionTask bileşenleri kullanılır.")]
        [SerializeField] private L1_MissionTask[] tasks = new L1_MissionTask[0];
        [Tooltip("Ana menü sahnesi.")]
        [SerializeField] private string firstLevelScene = "Level1";
        [SerializeField] private float pollInterval = 0.2f;
        [SerializeField] private float criticalTiltWarning = 16f;
        [Tooltip("Pencere/elektrik gibi tehlike noktasına bu mesafeden yakın olmak güvensiz sayılır (m).")]
        [SerializeField] private float hazardDistance = 1.6f;

        private MissionManager missions;
        private WilsonPortrait portrait;
        private WilsonBriefingPanel briefing;
        private MissionHudPanel hud;
        private DisasterResultPanel result;
        private L1_MissionSwitch[] switches = new L1_MissionSwitch[0];
        private float pollClock, tiltWarnClock;
        private bool timeWarned, started;
        private static bool retrying;

        private LevelDefinition Level => game != null ? game.levelDefinition : null;

        private void Awake()
        {
            if (game == null) game = GetComponent<L1_ProjectGameController>();
            if (drag == null && game != null) drag = game.drag;
        }

        private IEnumerator Start()
        {
            if (game == null || !game.missionMode || Level == null) { enabled = false; yield break; }
            // Kontrolcü Start'ta bölümü kurar; bir kare bekleyip hazır durumu kullan.
            yield return null;
            missions = game.missions;
            if (tasks == null || tasks.Length == 0) tasks = game.houseRoot.GetComponentsInChildren<L1_MissionTask>(true);
            switches = game.houseRoot.GetComponentsInChildren<L1_MissionSwitch>(true);
            foreach (var s in switches)
            {
                s.ResetSwitch();
                s.Changed += OnSwitchChanged;
            }

            portrait = WilsonPortrait.Create(transform, BuildPortraitModel);
            hud = MissionHudPanel.Create(transform, portrait);
            briefing = WilsonBriefingPanel.Create(transform, portrait);
            result = DisasterResultPanel.Create(transform, portrait);
            hud.Bind(missions);
            hud.HintRequested = ShowHint;
            hud.ReplayRequested = () => OpenBriefing(true);
            result.RetryRequested = () => { retrying = true; game.UiRestart(); };
            result.NextRequested = () => { retrying = false; game.UiNextLevel(); };
            result.MenuRequested = () => { retrying = false; game.UiMainMenu(); };
            result.ReplayAllRequested = () => { retrying = false; game.UiLoadScene(firstLevelScene); };

            missions.TaskCompleted += OnTaskCompleted;
            game.DisasterStarted += OnDisasterStarted;
            game.LevelFinished += OnLevelFinished;
            if (drag != null) { drag.allowCarry = true; drag.carryOnlyMarked = true; drag.interactOverride = TryUseSwitch; }

            OpenBriefing(!retrying || !LevelProgressSave.BriefingSeen(Level.levelNumber));
            retrying = false;
        }

        /// <summary>Deprem çantası noktası: E ile alınınca çanta yerden kalkar, karakterin sırtına takılır.</summary>
        private void OnSwitchChanged(L1_MissionSwitch s)
        {
            if (s == null || !s.Activated || s.switchId != "EmergencyBag") return;
            var model = s.offVisual != null ? s.offVisual.transform.Find("Çanta modeli") : null;
            game.EquipEmergencyBag(model);
        }

        private Material BuildPortraitModel(Transform host) =>
            game.wilsonModel != null ? L1_WilsonChicken.BuildPortraitModel(game.wilsonModel, game.wilsonTexture, host, 0.62f) : null;

        private void OnDestroy()
        {
            if (missions != null) missions.TaskCompleted -= OnTaskCompleted;
            foreach (var s in switches) if (s != null) s.Changed -= OnSwitchChanged;
            if (game != null) { game.DisasterStarted -= OnDisasterStarted; game.LevelFinished -= OnLevelFinished; }
            if (drag != null && drag.interactOverride == (System.Func<bool>)TryUseSwitch) drag.interactOverride = null;
        }

        // ------------------------------------------------------------------ brifing
        private void OpenBriefing(bool fullFlow)
        {
            if (!game.IsPlanning) return;
            game.SetBriefingOpen(true);
            hud.SetVisible(false);
            hud.briefingUsesPortrait = true;
            briefing.Open(Level, fullFlow, OnBriefingClosed);
        }

        private void OnBriefingClosed()
        {
            hud.briefingUsesPortrait = false;
            game.SetBriefingOpen(false);
            hud.SetVisible(true);
            LevelProgressSave.MarkBriefingSeen(Level.levelNumber);
            if (!started)
            {
                started = true;
                var ready = Level.dialogue != null ? Level.dialogue.First(WilsonTrigger.PrepStart) : null;
                if (ready != null) hud.Say(ready.textTr, ready.mood, 3f);
            }
        }

        // ------------------------------------------------------------------ döngü
        private void Update()
        {
            if (missions == null || game == null) return;
            if (game.IsPlanning && !game.IsBriefingOpen && !game.IsPausedMenu)
            {
                pollClock += Time.deltaTime;
                if (pollClock >= pollInterval)
                {
                    var dt = pollClock;
                    pollClock = 0f;
                    foreach (var task in tasks)
                        if (task != null && missions.StateOf(task.taskId) == TaskState.Pending &&
                            task.Evaluate(game, drag, dt))
                            missions.Complete(task.taskId);
                }
                if (!timeWarned && game.TimeLeft <= Level.timeWarningSeconds)
                {
                    timeWarned = true;
                    var line = Level.dialogue != null ? Level.dialogue.First(WilsonTrigger.TimeWarning) : null;
                    if (line != null) hud.Say(line.textTr, line.mood, 3f);
                }
                tiltWarnClock -= Time.deltaTime;
                if (Mathf.Abs(game.TiltDegrees) >= criticalTiltWarning && tiltWarnClock <= 0f)
                {
                    tiltWarnClock = 10f;
                    hud.Say("Denge çok bozuldu! Önce evi toparla!", WilsonMood.Endiseli, 2.5f);
                }
            }
            else if (game.IsDisaster) game.AccumulateBehaviour(IsBehaviourSafe(), Time.deltaTime);
        }

        private bool IsBehaviourSafe()
        {
            var resident = game.resident;
            if (resident == null) return false;
            switch (Level.disaster)
            {
                case LevelDisaster.Earthquake:
                    return resident.IsHiding;
                case LevelDisaster.HeavyRain:
                {
                    var feet = game.houseRoot.InverseTransformPoint(resident.transform.position).y - resident.footOffset;
                    return feet > game.FloorTops()[0] + 1f;
                }
                case LevelDisaster.Storm:
                case LevelDisaster.Lightning:
                    foreach (var s in switches)
                        if (s != null && s.hazardDuringDisaster && s.InReach(game.houseRoot, resident) &&
                            Mathf.Abs(game.houseRoot.InverseTransformPoint(s.transform.position).x -
                                      game.houseRoot.InverseTransformPoint(resident.transform.position).x) < hazardDistance)
                            return false;
                    return true;
                default:
                    return true;
            }
        }

        private void OnTaskCompleted(TaskDefinition task)
        {
            game.GrantProtection(task.protectionKey);
            foreach (var t in tasks) if (t != null && t.taskId == task.id) t.OnCompleted();
            hud.Say(task.completionTr, WilsonMood.Sevincli, 2.2f);
        }

        private void OnDisasterStarted()
        {
            foreach (var s in switches) if (s != null) s.Locked = true;
            hud.SetButtonsVisible(false);
            var line = Level.dialogue != null ? Level.dialogue.First(WilsonTrigger.DisasterWarning) : null;
            if (line != null) hud.Say(line.textTr, line.mood, 3.5f);
        }

        private void OnLevelFinished(L1_ProjectGameController.MissionResult r)
        {
            hud.SetVisible(false);
            var d = Level.dialogue;
            var good = r.survived ? d?.First(WilsonTrigger.ReportGood) : d?.First(WilsonTrigger.ReportBad);
            var final = d?.First(WilsonTrigger.Final);
            result.Show(new DisasterResultPanel.Data
            {
                levelTitle = "BÖLÜM " + Level.levelNumber.ToString("00") + " · " + Level.titleTr,
                survived = r.survived,
                failReason = r.failReason,
                stars = r.stars,
                score = r.score,
                preparation = r.preparation,
                behaviour = r.behaviour,
                balance = r.balance,
                protection = r.protection,
                lines = r.lines,
                wilsonLine = good != null ? good.textTr : "",
                wilsonMood = good != null ? good.mood : WilsonMood.Normal,
                realLifeTip = Level.realLifeTipTr,
                hasNext = game.HasNextScene,
                isFinal = !game.HasNextScene,
                finalLine = final != null ? final.textTr : ""
            });
        }

        // ------------------------------------------------------------------ etkileşim
        /// <summary>E: yakında görev noktası varsa onu kullanır (taşıma yerine). Tek giriş sözleşmesi.</summary>
        private bool TryUseSwitch()
        {
            if (!game.IsPlanning || game.IsBriefingOpen) return false;
            L1_MissionSwitch best = null;
            var bestDistance = float.MaxValue;
            var player = game.houseRoot.InverseTransformPoint(game.resident.transform.position);
            foreach (var s in switches)
            {
                if (s == null || !s.interactable || s.Activated || !s.InReach(game.houseRoot, game.resident)) continue;
                var d = Mathf.Abs(game.houseRoot.InverseTransformPoint(s.transform.position).x - player.x);
                if (d < bestDistance) { bestDistance = d; best = s; }
            }
            if (best == null) return false;
            if (!best.PrerequisiteMet)
            {
                hud.Say("Orası hazır değil; önce gerekli eşyayı işaretli yere bırak.", WilsonMood.Anlatici, 2.5f);
                return true;
            }
            if (best.TryActivate()) hud.Say(best.doneTr, WilsonMood.Normal, 1.5f);
            return true;
        }

        private void ShowHint()
        {
            if (Level == null) return;
            TaskDefinition next = null;
            foreach (var task in Level.tasks)
                if (task != null && missions.StateOf(task.id) == TaskState.Pending && missions.PrerequisitesMet(task)) { next = task; break; }
            if (next == null)
            {
                hud.Say(missions.CompletedCount == missions.TotalCount ? "Bütün görevler tamam! BOŞLUK ile afete hazır olduğunu söyle."
                    : "Önce diğer görevleri bitir.", WilsonMood.Sevincli, 3f);
                return;
            }
            hud.Say(next.hintTr, WilsonMood.Anlatici, 4f);
            foreach (var t in tasks)
            {
                if (t == null || t.taskId != next.id) continue;
                if (t.zone != null) t.zone.Glow(3f);
                foreach (var z in t.HintTarget.GetComponentsInChildren<L1_MissionZone>()) z.Glow(3f);
            }
        }
    }
}
