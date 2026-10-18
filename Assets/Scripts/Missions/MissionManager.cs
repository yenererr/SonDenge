using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jam.Missions
{
    /// <summary>
    /// Bir bölümün görev durumlarının tek doğruluk kaynağı. Görevler yalnız doğrulanmış
    /// dünya olaylarından (Complete çağrısı) tamamlanır; aynı görev ikinci kez puan vermez.
    /// </summary>
    public sealed class MissionManager : MonoBehaviour
    {
        [SerializeField] private LevelDefinition level;

        private readonly Dictionary<string, TaskState> states = new Dictionary<string, TaskState>();
        private bool locked;

        public LevelDefinition Level => level;
        public event Action<TaskDefinition> TaskCompleted;
        public event Action TasksLocked;

        public bool IsLocked => locked;

        private void Awake() => ResetTasks();

        public void SetLevel(LevelDefinition definition)
        {
            level = definition;
            ResetTasks();
        }

        /// <summary>Bölüm (yeniden) başlarken bütün görevleri Pending yapar.</summary>
        public void ResetTasks()
        {
            states.Clear();
            locked = false;
            if (level == null) return;
            foreach (var task in level.tasks)
                if (task != null && !string.IsNullOrEmpty(task.id)) states[task.id] = TaskState.Pending;
        }

        public TaskState StateOf(string id) => states.TryGetValue(id, out var s) ? s : TaskState.Pending;
        public bool IsCompleted(string id) => StateOf(id) == TaskState.Completed;

        public bool PrerequisitesMet(TaskDefinition task)
        {
            if (task == null) return false;
            foreach (var pre in task.prerequisiteTaskIds)
                if (!string.IsNullOrEmpty(pre) && !IsCompleted(pre)) return false;
            return true;
        }

        /// <summary>Görevi tamamlar. Kilitliyse, ön koşul eksikse veya zaten tamamsa false döner.</summary>
        public bool Complete(string id)
        {
            if (locked || level == null) return false;
            var task = level.FindTask(id);
            if (task == null || !states.ContainsKey(id)) return false;
            if (states[id] != TaskState.Pending || !PrerequisitesMet(task)) return false;
            states[id] = TaskState.Completed;
            TaskCompleted?.Invoke(task);
            return true;
        }

        /// <summary>Afet başlarken çağrılır: tamamlanmamış hazırlık görevleri Missed olur.</summary>
        public void LockForDisaster()
        {
            if (locked) return;
            locked = true;
            if (level != null)
                foreach (var task in level.tasks)
                    if (task != null && states.TryGetValue(task.id, out var s) && s == TaskState.Pending)
                        states[task.id] = TaskState.Missed;
            TasksLocked?.Invoke();
        }

        public int CompletedCount
        {
            get { var n = 0; foreach (var s in states.Values) if (s == TaskState.Completed) n++; return n; }
        }

        public int TotalCount => states.Count;

        public bool AllCriticalCompleted
        {
            get
            {
                if (level == null) return false;
                foreach (var task in level.tasks)
                    if (task != null && task.safetyCritical && !IsCompleted(task.id)) return false;
                return true;
            }
        }

        /// <summary>Hazırlık puanı (0..35), görev ağırlıklarına göre.</summary>
        public float PreparationPoints => MissionScoring.PreparationPoints(level, IsCompleted);

        public bool HasProtection(string key)
        {
            if (level == null || string.IsNullOrEmpty(key)) return false;
            foreach (var task in level.tasks)
                if (task != null && task.protectionKey == key && IsCompleted(task.id)) return true;
            return false;
        }
    }
}
