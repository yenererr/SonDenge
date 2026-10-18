using UnityEngine;

namespace Jam.Missions
{
    /// <summary>
    /// Tek bir hazırlık görevinin verisi. Tamamlanma koşulu sahnedeki bağlayıcı bileşen
    /// (ör. L1_MissionTask) tarafından dünya içi etkileşimle doğrulanır; UI yalnız sonucu gösterir.
    /// </summary>
    [CreateAssetMenu(menuName = "SonDenge/Task Definition", fileName = "L1_T01")]
    public sealed class TaskDefinition : ScriptableObject
    {
        [Tooltip("Benzersiz kimlik, ör. L1_T01.")]
        public string id;
        public string levelId;
        public string titleTr;
        [TextArea(1, 3)] public string descriptionTr;
        [TextArea(1, 3)] public string whyItMattersTr;
        [TextArea(1, 3)] public string hintTr;
        [Tooltip("Wilson'ın görev tamamlanınca söylediği kısa cümle.")]
        [TextArea(1, 2)] public string completionTr;
        [Tooltip("Afet raporunda, görev gerçekten işe yaradıysa yazılan cümle.")]
        [TextArea(1, 2)] public string reportTr;
        public TaskType taskType;
        public string[] prerequisiteTaskIds = new string[0];
        [Tooltip("Afeti güvenli atlatmak için kritik mi? (3 yıldız koşulu)")]
        public bool safetyCritical;
        [Tooltip("Göreceli ağırlık; toplam hazırlık puanı 35'e normalize edilir.")]
        public float scoreWeight = 1f;
        [Tooltip("Görev tamamlanınca afet simülasyonunda açılan koruma anahtarı (ör. storm_brace).")]
        public string protectionKey;
    }
}
