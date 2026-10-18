using System.Globalization;
using Jam.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Bölüm öncesi Wilson görev tanıtımı (referans 03). Görev listesi LevelDefinition.tasks'tan okunur;
    /// hazırlık süresi burada başlamaz, HAZIRLIĞA BAŞLA ile yükleme ekranına geçilir.
    /// </summary>
    public sealed class BriefingView : MenuPage
    {
        [SerializeField] private TMP_Text chapterNumberText;
        [SerializeField] private TMP_Text chapterNameText;
        [SerializeField] private TMP_Text wilsonText;
        [SerializeField] private RectTransform taskList;
        [SerializeField] private TaskRowView taskRowPrefab;
        [SerializeField] private Sprite fallbackTaskIcon;
        [SerializeField] private TMP_Text safetyText;
        [SerializeField] private GameObject dropCoverStrip;
        [SerializeField] private Image safetyIcon;
        [SerializeField] private Button startButton;
        [SerializeField] private Button backButton;

        private static readonly CultureInfo Tr = new CultureInfo("tr-TR");

        protected override void Awake()
        {
            base.Awake();
            startButton.onClick.AddListener(() => Flow.StartChapter(Flow.SelectedChapter));
            backButton.onClick.AddListener(() => Flow.Back());
        }

        protected override void OnShown()
        {
            var index = Flow.SelectedChapter;
            var def = Flow.Levels[index];
            var info = Flow.Chapter(index);
            chapterNumberText.text = def.levelNumber + ". BÖLÜM";
            chapterNameText.text = def.disasterNameTr;
            wilsonText.text = info.briefingLine;
            safetyText.text = info.safetyNote;
            dropCoverStrip.SetActive(info.showDropCoverHold);
            if (safetyIcon != null)
            {
                safetyIcon.gameObject.SetActive(!info.showDropCoverHold && info.disasterIcon != null);
                safetyIcon.sprite = info.disasterIcon;
            }
            for (var i = taskList.childCount - 1; i >= 0; i--) Destroy(taskList.GetChild(i).gameObject);
            for (var i = 0; i < def.tasks.Length; i++)
            {
                var task = def.tasks[i];
                if (task == null) continue;
                var row = Instantiate(taskRowPrefab, taskList);
                var icon = i < info.taskIcons.Length && info.taskIcons[i] != null ? info.taskIcons[i] : fallbackTaskIcon;
                row.Set(icon, SentenceCase(task.titleTr));
            }
            if (Flow.Wilson != null) Flow.Wilson.Say(WilsonMood.Anlatici, 2.5f);
        }

        /// <summary>"KİTAPLIĞI SABİTLE" → "Kitaplığı sabitle" (Türkçe kültürle).</summary>
        public static string SentenceCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var lower = text.ToLower(Tr);
            return char.ToUpper(lower[0], Tr) + lower.Substring(1);
        }
    }
}
