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
        [Tooltip("Beş bölümün ortak AFET RAPORU prefab'ı (UI_DisasterReportTemplate). Boşsa eski kodla kurulan rapor kullanılır.")]
        [SerializeField] private DisasterReportPanel reportPrefab;
        [Tooltip("Menü tasarım dilindeki Duraklat menüsü (UI_PauseMenu). Boşsa eski kodla kurulan panel kullanılır.")]
        [SerializeField] private Jam.UI.Menu.PauseMenuView pauseMenuPrefab;

        private MissionManager missions;
        private WilsonPortrait portrait;
        private WilsonBriefingPanel briefing;
        private MissionHudPanel hud;
        private DisasterResultPanel result;
        private DisasterReportPanel report;
        private Jam.UI.Menu.PauseMenuView pauseMenu;
        private L1_TaskGuide guide;
        [Tooltip("Karakter hedefe bu yatay mesafeden yakınsa tuş ipucu görünür (m).")]
        [SerializeField] private float promptDistance = 1.5f;
        private L1_MissionSwitch[] switches = new L1_MissionSwitch[0];
        private float pollClock, tiltWarnClock;
        private bool timeWarned, started;
        private static bool retrying;
        private static bool forceFullBriefing;

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
            if (reportPrefab != null)
            {
                report = Instantiate(reportPrefab, transform);
                report.BindPortrait(portrait);
            }
            else result = DisasterResultPanel.Create(transform, portrait);
            if (pauseMenuPrefab != null)
            {
                pauseMenu = Instantiate(pauseMenuPrefab, transform);
                pauseMenu.IsPauseOpen = () => game.IsMenuOpen && !game.IsHowToOpen;
                pauseMenu.SubMenuChanged = open => game.PauseSubMenuOpen = open;
                pauseMenu.Resume = game.UiTogglePause;
                pauseMenu.Restart = game.UiRestart;
                pauseMenu.MainMenu = game.UiMainMenu;
                game.ExternalPauseMenu = true;
            }
            // ESC'nin tek sahibi bölüm kontrolcüsü: GameManager ikinci kez duraklatmasın.
            if (Jam.Core.GameManager.Instance != null) Jam.Core.GameManager.Instance.PauseInputEnabled = false;
            guide = L1_TaskGuide.Create(transform);
            hud.Bind(missions);
            hud.HintRequested = ShowHint;
            hud.ReplayRequested = () => OpenBriefing(true);
            if (result != null)
            {
                result.RetryRequested = Retry;
                result.NextRequested = Next;
                result.MenuRequested = Menu;
                result.ReplayAllRequested = ReplayAll;
            }

            missions.TaskCompleted += OnTaskCompleted;
            game.DisasterStarted += OnDisasterStarted;
            game.LevelFinished += OnLevelFinished;
            if (drag != null) { drag.allowCarry = true; drag.carryOnlyMarked = true; drag.interactOverride = TryUseSwitch; }

            OpenBriefing(!retrying || forceFullBriefing || !LevelProgressSave.BriefingSeen(Level.levelNumber));
            retrying = false;
            forceFullBriefing = false;
        }

        /// <summary>Deprem çantası noktası: E ile alınınca çanta yerden kalkar, karakterin sırtına takılır.</summary>
        private void OnSwitchChanged(L1_MissionSwitch s)
        {
            if (s == null || !s.Activated || s.switchId != "EmergencyBag") return;
            var model = s.offVisual != null ? s.offVisual.transform.Find("Çanta modeli") : null;
            game.EquipEmergencyBag(model);
        }

        private void Retry() { retrying = true; game.UiRestart(); }
        private void RetryWithBriefing() { retrying = true; forceFullBriefing = true; game.UiRestart(); }
        private void Next() { retrying = false; game.UiNextLevel(); }
        private void Menu() { retrying = false; game.UiMainMenu(); }
        private void ReplayAll() { retrying = false; game.UiLoadScene(firstLevelScene); }

        private Material BuildPortraitModel(Transform host) =>
            game.wilsonModel != null ? L1_WilsonChicken.BuildPortraitModel(game.wilsonModel, game.wilsonTexture, host, 0.62f) : null;

        private void OnDestroy()
        {
            if (Jam.Core.GameManager.Instance != null) Jam.Core.GameManager.Instance.PauseInputEnabled = true;
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
            game.PlayRecordedSfx("wilson_voice", 0.9f);
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
                    // Görevler sırayla açılır: yalnız sıradaki görev değerlendirilir.
                    var current = CurrentTask;
                    var task = current != null ? TaskObject(current.id) : null;
                    if (task != null && task.Evaluate(game, drag, dt)) missions.Complete(current.id);
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
            UpdateGuide();
        }

        // ------------------------------------------------------------------ sıralı görevler + ok
        /// <summary>Bölüm sırasındaki ilk bekleyen görev (yalnız o açıktır).</summary>
        public TaskDefinition CurrentTask
        {
            get
            {
                if (Level == null || missions == null || missions.IsLocked) return null;
                foreach (var task in Level.tasks)
                    if (task != null && missions.StateOf(task.id) == TaskState.Pending) return task;
                return null;
            }
        }

        private L1_MissionTask TaskObject(string id)
        {
            foreach (var t in tasks) if (t != null && t.taskId == id) return t;
            return null;
        }

        private void UpdateGuide()
        {
            if (guide == null) return;
            var current = game.IsPlanning && !game.IsBriefingOpen && !game.IsPausedMenu ? CurrentTask : null;
            var task = current != null ? TaskObject(current.id) : null;
            hud.SetCurrent(CurrentTask != null ? CurrentTask.id : null);
            if (task == null || !ResolveGuide(task, out var target, out var key))
            {
                guide.Show(null, null);
                return;
            }
            guide.Show(target, IsNear(target) ? key : null);
        }

        private bool IsNear(Vector3 world)
        {
            if (game.resident == null) return false;
            var player = game.houseRoot.InverseTransformPoint(game.resident.transform.position);
            var target = game.houseRoot.InverseTransformPoint(world);
            var feet = player.y - game.resident.footOffset;
            return Mathf.Abs(player.x - target.x) <= promptDistance && target.y > feet - 0.6f && target.y < feet + 2.3f;
        }

        /// <summary>Görevin şu anki adımı için okun gideceği yer ve basılacak tuş.</summary>
        private bool ResolveGuide(L1_MissionTask task, out Vector3 target, out string key)
        {
            target = task.HintTarget.position;
            key = null;
            var carried = drag != null ? drag.Carried : null;
            switch (task.condition)
            {
                case L1_MissionTask.Condition.Switches:
                    foreach (var s in task.switches)
                    {
                        if (s == null || s.Activated) continue;
                        if (s.requiresItemInZone != null && s.requiredItem != null && !s.PrerequisiteMet)
                            return ItemStep(s.requiredItem, s.requiresItemInZone, carried, out target, out key);
                        target = s.transform.position;
                        key = "E — " + Capital(s.actionTr);
                        return true;
                    }
                    return false;
                case L1_MissionTask.Condition.Anchored:
                    foreach (var item in task.items)
                    {
                        if (item == null || item.IsAnchored) continue;
                        target = item.transform.position;
                        key = "Sol tık ile seç · F — Sabitle";
                        return true;
                    }
                    return false;
                case L1_MissionTask.Condition.ItemsInZone:
                    foreach (var item in task.items)
                        if (item != null && item.Body != null && (carried == item || !task.zone.Contains(item.Body.position)))
                            return ItemStep(item, task.zone, carried, out target, out key);
                    target = task.zone.transform.position;
                    return true;
                case L1_MissionTask.Condition.ZoneClear:
                    foreach (var item in task.items)
                    {
                        if (item == null || item.Body == null || !item.gameObject.activeInHierarchy) continue;
                        if (carried == item) { target = item.transform.position; key = "Alandan uzaklaş · E — Bırak"; return true; }
                        if (!task.zone.Contains(item.Body.position)) continue;
                        target = item.transform.position;
                        key = item.carryable ? "E — Al ve alanın dışına bırak" : "Sol tık ile sürükleyip alandan çıkar";
                        return true;
                    }
                    return false;
                case L1_MissionTask.Condition.Balance:
                    key = "Ağırlıkları evin ortasına taşı";
                    return true;
            }
            return false;
        }

        private static bool ItemStep(L1_MovableProp item, L1_MissionZone zone, L1_MovableProp carried, out Vector3 target, out string key)
        {
            if (carried == item)
            {
                target = zone.transform.position;
                key = "E — Buraya bırak";
                return true;
            }
            target = item.transform.position;
            key = item.carryable ? "E — " + (string.IsNullOrEmpty(item.displayName) ? "Eşyayı al" : item.displayName + " al")
                : "Sol tık ile sürükleyip işaretli yere götür";
            return true;
        }

        private static readonly System.Globalization.CultureInfo Tr = new System.Globalization.CultureInfo("tr-TR");

        private static string Capital(string text) =>
            string.IsNullOrEmpty(text) ? "Kullan" : char.ToUpper(text[0], Tr) + text.Substring(1).ToLower(Tr);

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
            var next = CurrentTask;
            hud.Say(next != null ? task.completionTr + " Sıradaki: " + Jam.UI.Menu.BriefingView.SentenceCase(next.titleTr)
                : task.completionTr + " Bütün görevler tamam!", WilsonMood.Sevincli, 3f);
        }

        private void OnDisasterStarted()
        {
            if (guide != null) guide.Show(null, null);
            foreach (var s in switches) if (s != null) s.Locked = true;
            hud.SetButtonsVisible(false);
            var line = Level.dialogue != null ? Level.dialogue.First(WilsonTrigger.DisasterWarning) : null;
            if (line != null) hud.Say(line.textTr, line.mood, 3.5f);
        }

        private void OnLevelFinished(L1_ProjectGameController.MissionResult r)
        {
            hud.SetVisible(false);
            hud.briefingUsesPortrait = true; // rapor açıkken portre kamerası çalışmaya devam etsin
            var d = Level.dialogue;
            var good = r.survived ? d?.First(WilsonTrigger.ReportGood) : d?.First(WilsonTrigger.ReportBad);
            var final = d?.First(WilsonTrigger.Final);
            if (report != null) { ShowReport(r, good, final); return; }
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

        /// <summary>Ortak şablon prefab'ına bölüm verisini aktarır; düğmeler sonuca göre değişir.</summary>
        private void ShowReport(L1_ProjectGameController.MissionResult r, WilsonLine good, WilsonLine final)
        {
            var lines = new System.Collections.Generic.List<DisasterReportPanel.Line>();
            if (!r.survived && !string.IsNullOrEmpty(r.failReason))
                lines.Add(new DisasterReportPanel.Line(DisasterReportPanel.LineKind.Critical, "Neden: " + r.failReason));
            foreach (var raw in r.lines)
            {
                if (string.IsNullOrEmpty(raw)) continue;
                var kind = raw.StartsWith("●") ? DisasterReportPanel.LineKind.Success :
                    raw.StartsWith("×") ? DisasterReportPanel.LineKind.Missing : DisasterReportPanel.LineKind.Info;
                var text = kind == DisasterReportPanel.LineKind.Info ? raw : raw.Substring(1).TrimStart();
                lines.Add(new DisasterReportPanel.Line(kind, text));
            }
            var isFinal = !game.HasNextScene;
            if (isFinal && r.survived)
            {
                var summary = "Beş bölüm:";
                for (var i = 1; i <= LevelProgressSave.LevelCount; i++)
                    summary += "  " + i + " " + new string('●', LevelProgressSave.BestStars(i)) + new string('○', 3 - LevelProgressSave.BestStars(i));
                lines.Add(new DisasterReportPanel.Line(DisasterReportPanel.LineKind.Info, summary + "  —  AFETE HAZIRLIK HAYAT KURTARIR."));
            }
            var wilson = good != null ? good.textTr : "";
            if (isFinal && r.survived && final != null) wilson += " " + final.textTr;
            var data = new DisasterReportPanel.Data
            {
                chapterTitle = "BÖLÜM " + Level.levelNumber.ToString("00") + " · " + Level.titleTr + " — " + Level.disasterNameTr,
                survived = r.survived,
                statusText = r.survived ? "EV AYAKTA KALDI" : "DENGE BOZULDU",
                totalScore = r.score,
                stars = r.stars,
                preparation = r.preparation,
                behaviour = r.behaviour,
                balance = r.balance,
                protection = r.protection,
                maxTiltDegrees = r.maxTilt,
                collapseAngle = game.CollapseAngle,
                lines = lines,
                wilsonMessage = wilson,
                wilsonMood = good != null ? good.mood : WilsonMood.Normal,
                safetyNote = "Gerçek hayatta: " + StripPrefix(Level.realLifeTipTr) + "  ·  Evin ağırlıkla dengelenmesi oyunun hayal ürünü kuralıdır.",
                rightLabel = "ANA MENÜ",
                onRight = Menu
            };
            if (!r.survived)
            {
                data.leftLabel = "WILSON'I TEKRAR DİNLE";
                data.onLeft = RetryWithBriefing;
                data.primaryLabel = "BU BÖLÜMÜ TEKRAR DENE";
                data.onPrimary = Retry;
            }
            else
            {
                data.leftLabel = "BU BÖLÜMÜ TEKRAR DENE";
                data.onLeft = Retry;
                data.primaryLabel = isFinal ? "YENİDEN OYNA" : "SONRAKİ BÖLÜM";
                data.onPrimary = isFinal ? (System.Action)ReplayAll : Next;
            }
            report.Show(data);
        }

        private static string StripPrefix(string tip) =>
            string.IsNullOrEmpty(tip) ? "" : tip.StartsWith("Gerçek hayatta:") ? tip.Substring("Gerçek hayatta:".Length).TrimStart() : tip;

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
            var current = CurrentTask;
            var currentTask = current != null ? TaskObject(current.id) : null;
            if (currentTask == null || System.Array.IndexOf(currentTask.switches, best) < 0)
            {
                hud.Say(current != null ? "Önce sıradaki görev: " + Jam.UI.Menu.BriefingView.SentenceCase(current.titleTr)
                    : "Burada şimdilik yapılacak bir şey yok.", WilsonMood.Anlatici, 2.5f);
                return true;
            }
            if (!best.PrerequisiteMet)
            {
                hud.Say("Orası hazır değil; önce gerekli eşyayı işaretli yere bırak.", WilsonMood.Anlatici, 2.5f);
                return true;
            }
            if (best.TryActivate())
            {
                hud.Say(best.doneTr, WilsonMood.Normal, 1.5f);
                if (best.actionTr != null && best.actionTr.ToUpperInvariant().Contains("PENCERE")) game.PlayRecordedSfx("window_close");
            }
            return true;
        }

        private void ShowHint()
        {
            if (Level == null) return;
            TaskDefinition next = null;
            next = CurrentTask;
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
