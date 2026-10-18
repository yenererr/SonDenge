using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>Ana menü (referans 01): OYUNA BAŞLA, NASIL OYNANIR?, AYARLAR, ÇIKIŞ + Oyun Hakkında bağlantısı.</summary>
    public sealed class MainMenuView : MenuPage
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button howToButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button aboutButton;
        [SerializeField] private TMP_Text playLabel;
        [SerializeField] private TMP_Text versionText;
        [SerializeField] private TMP_Text wilsonText;
        [SerializeField, TextArea(2, 3)] private string greeting = "Merhaba! Ben Wilson! Afetlere karşı hazırlanmaya var mısın?";
        [SerializeField, TextArea(2, 3)] private string returningGreeting = "Tekrar hoş geldin! Evimizi bir sonraki afete birlikte hazırlayalım.";

        protected override void Awake()
        {
            base.Awake();
            playButton.onClick.AddListener(() => Flow.Open(MenuPageId.ChapterSelect));
            howToButton.onClick.AddListener(() => Flow.Open(MenuPageId.Help));
            settingsButton.onClick.AddListener(() => Flow.Open(MenuPageId.Settings));
            quitButton.onClick.AddListener(() => Flow.RequestQuit());
            if (aboutButton != null) aboutButton.onClick.AddListener(() => Flow.Open(MenuPageId.About));
#if UNITY_WEBGL && !UNITY_EDITOR
            quitButton.gameObject.SetActive(false); // tarayıcıda Application.Quit çalışmaz
#endif
            if (versionText != null) versionText.text = "v" + Application.version;
        }

        protected override void OnShown()
        {
            var returning = Flow.AnyProgress();
            if (wilsonText != null) wilsonText.text = returning ? returningGreeting : greeting;
            if (playLabel != null) playLabel.text = returning ? "OYUNA DEVAM ET" : "OYUNA BAŞLA";
            if (Flow.Wilson != null) Flow.Wilson.Greet();
        }
    }
}
