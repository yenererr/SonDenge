using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Oyun Hakkında / Krediler (referans 07). TEŞEKKÜRLER sağ sütunu ekip ↔ teşekkür/kaynaklar arasında değiştirir.</summary>
    public sealed class AboutView : MenuPage
    {
        [SerializeField] private Button thanksButton;
        [SerializeField] private TMP_Text thanksLabel;
        [SerializeField] private Button backButton;
        [SerializeField] private GameObject teamPanel;
        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private TMP_Text sideTitle;
        [SerializeField] private ScrollRect creditsScroll;

        private bool showingCredits;

        protected override void Awake()
        {
            base.Awake();
            thanksButton.onClick.AddListener(() => ShowCredits(!showingCredits));
            backButton.onClick.AddListener(() => Flow.Back());
        }

        protected override void OnShown() => ShowCredits(false);

        public override bool HandleBack()
        {
            if (!showingCredits) return false;
            ShowCredits(false);
            return true;
        }

        private void ShowCredits(bool on)
        {
            showingCredits = on;
            teamPanel.SetActive(!on);
            creditsPanel.SetActive(on);
            sideTitle.text = on ? "Teşekkürler" : "Ekibimiz";
            thanksLabel.text = on ? "Ekibimiz" : "Teşekkürler";
            if (on && creditsScroll != null) creditsScroll.verticalNormalizedPosition = 1f;
        }
    }
}
