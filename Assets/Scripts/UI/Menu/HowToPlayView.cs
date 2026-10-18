using Jam.Missions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Nasıl Oynanır? (referans 05): altı sekme, iki sütun içerik, kaydırma alanı, Wilson ipucu.
    /// Kontrol metinleri oyundaki gerçek tuşlarla eşleşir (L1_ProjectGameController / L1_ResidentController).
    /// </summary>
    public sealed class HowToPlayView : MenuPage
    {
        [SerializeField] private TabButtonView[] tabs = new TabButtonView[0];
        [SerializeField] private TMP_Text heading;
        [SerializeField] private RectTransform leftColumn;
        [SerializeField] private RectTransform rightColumn;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private HelpRowView rowPrefab;
        [SerializeField] private TMP_Text wilsonText;
        [SerializeField] private Button backButton;
        [SerializeField] private HelpSection[] sections = new HelpSection[0];

        private int active = -1;

        protected override void Awake()
        {
            base.Awake();
            for (var i = 0; i < tabs.Length; i++)
            {
                var index = i;
                if (i < sections.Length) tabs[i].Set(sections[i].tabTitle, sections[i].tabIcon);
                tabs[i].Button.onClick.AddListener(() => SelectTab(index));
            }
            backButton.onClick.AddListener(() => Flow.Back());
        }

        protected override void OnShown()
        {
            active = -1;
            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= sections.Length || index == active) return;
            active = index;
            for (var i = 0; i < tabs.Length; i++) tabs[i].SetActive(i == index);
            var section = sections[index];
            heading.text = section.heading;
            wilsonText.text = section.wilsonLine;
            Clear(leftColumn);
            Clear(rightColumn);
            var half = Mathf.CeilToInt(section.items.Length / 2f);
            for (var i = 0; i < section.items.Length; i++)
                Instantiate(rowPrefab, i < half ? leftColumn : rightColumn).Set(section.items[i]);
            if (scroll != null) scroll.verticalNormalizedPosition = 1f;
            if (Flow != null)
            {
                Flow.Play(MenuSound.Click);
                if (Flow.Wilson != null) Flow.Wilson.Say(WilsonMood.Anlatici, 1.5f);
            }
        }

        private static void Clear(RectTransform column)
        {
            for (var i = column.childCount - 1; i >= 0; i--) Destroy(column.GetChild(i).gameObject);
        }

        private void Update()
        {
            if (!Flow || !Flow.IsCurrent(this)) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            var shift = kb.shiftKey.isPressed;
            if (kb.downArrowKey.wasPressedThisFrame || (kb.tabKey.wasPressedThisFrame && !shift))
                SelectTab((active + 1) % sections.Length);
            else if (kb.upArrowKey.wasPressedThisFrame || (kb.tabKey.wasPressedThisFrame && shift))
                SelectTab((active - 1 + sections.Length) % sections.Length);
        }
    }
}
