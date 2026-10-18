using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Bölüm Seçimi (referans 02): beş kart, ←/→ ile gezinme, Enter / DEVAM ET ile Wilson brifingi.
    /// Kilitli kart seçilebilir ama başlatılamaz; Wilson nedenini söyler.
    /// </summary>
    public sealed class ChapterSelectView : MenuPage
    {
        [SerializeField] private ChapterCardView[] cards = new ChapterCardView[0];
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text wilsonText;
        [SerializeField, TextArea(2, 3)] private string firstTimeLine =
            "İlk olarak deprem bölümünü oynayalım. Evimizi sarsıntıya karşı birlikte hazırlayacağız!";
        [SerializeField, TextArea(2, 3)] private string lockedLine = "Bu bölüm için önceki bölümü tamamla.";

        private int selected;

        protected override void Awake()
        {
            base.Awake();
            foreach (var card in cards)
            {
                card.Clicked = Select;
                card.DoubleClicked = i => { Select(i); Continue(); };
            }
            continueButton.onClick.AddListener(Continue);
            backButton.onClick.AddListener(() => Flow.Back());
        }

        protected override void OnShown()
        {
            for (var i = 0; i < cards.Length; i++)
            {
                var def = i < Flow.Levels.Length ? Flow.Levels[i] : null;
                cards[i].Setup(i, def, Flow.Chapter(i), Flow.IsUnlocked(i), Flow.BestStars(i));
            }
            Select(Mathf.Clamp(Flow.SelectedChapter, 0, cards.Length - 1), false);
        }

        private void Select(int index) => Select(index, true);

        private void Select(int index, bool sound)
        {
            selected = Mathf.Clamp(index, 0, cards.Length - 1);
            Flow.SelectedChapter = selected;
            for (var i = 0; i < cards.Length; i++) cards[i].SetSelected(i == selected);
            var unlocked = cards[selected].Unlocked;
            continueButton.interactable = unlocked;
            if (continueLabel != null) continueLabel.text = unlocked ? "Devam Et" : "Kilitli";
            var info = Flow.Chapter(selected);
            string line;
            if (!unlocked) line = lockedLine;
            else if (selected == 0 && !Flow.AnyProgress()) line = firstTimeLine;
            else line = info.selectLine;
            if (wilsonText != null) wilsonText.text = line;
            if (Flow.Wilson != null) Flow.Wilson.Say(unlocked ? Missions.WilsonMood.Anlatici : Missions.WilsonMood.Endiseli, 1.2f);
            if (sound) Flow.Play(MenuSound.Click);
            Focus();
        }

        private void Continue()
        {
            if (!cards[selected].Unlocked)
            {
                Flow.Play(MenuSound.Error);
                return;
            }
            Flow.Open(MenuPageId.Briefing);
        }

        private void Update()
        {
            if (!Flow || !Flow.IsCurrent(this)) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) Select(Mathf.Min(selected + 1, cards.Length - 1));
            if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) Select(Mathf.Max(selected - 1, 0));
        }
    }
}
