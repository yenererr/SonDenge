using UnityEngine;

namespace Jam.Missions
{
    /// <summary>Bir bölümün bütün ayarları: süreler, kat sayısı, görevler, Wilson metinleri.</summary>
    [CreateAssetMenu(menuName = "SonDenge/Level Definition", fileName = "L1_Deprem")]
    public sealed class LevelDefinition : ScriptableObject
    {
        public string levelId = "L1";
        [Min(1)] public int levelNumber = 1;
        public string sceneName = "Level1";
        [Tooltip("Boşsa bu son bölümdür.")]
        public string nextSceneName = "Level2";
        public string titleTr = "İLK SARSINTI";
        public string disasterNameTr = "DEPREM";
        public LevelDisaster disaster = LevelDisaster.Earthquake;
        [Range(2, 3)] public int floorCount = 2;
        [Min(1f)] public float preparationSeconds = 90f;
        [Min(1f)] public float disasterSeconds = 12f;
        [Tooltip("Kalan hazırlık süresi bu değerin altına inince Wilson bir kez uyarır.")]
        public float timeWarningSeconds = 20f;
        public TaskDefinition[] tasks = new TaskDefinition[0];
        public WilsonDialogueSet dialogue;
        [TextArea(1, 3)] public string[] safetyWarningsTr = new string[0];
        [Tooltip("Gerçek hayat güvenlik özeti (rapor ve briefing'de gösterilir).")]
        [TextArea(2, 4)] public string realLifeTipTr;

        public TaskDefinition FindTask(string id)
        {
            foreach (var task in tasks) if (task != null && task.id == id) return task;
            return null;
        }
    }
}
