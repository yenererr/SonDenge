using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Oyun içi Duraklat menüsü (menü tasarım dilinde). ESC'nin tek sahibi bölüm kontrolcüsüdür; bu görünüm yalnız
    /// IsPauseOpen'a göre açılır/kapanır ve alt durumlar (ayarlar, onay) açıkken ESC'yi kendisi işler.
    /// Time.timeScale = 0 iken animasyonlar unscaled çalışır.
    /// </summary>
    public sealed class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup root;
        [SerializeField] private RectTransform window;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private ConfirmDialog confirm;
        [SerializeField] private SettingsView settings;

        /// <summary>Bölüm kontrolcüsü duraklatma menüsü açık mı?</summary>
        public Func<bool> IsPauseOpen;
        /// <summary>Alt panel (ayarlar/onay) açıkken kontrolcünün ESC'yi yok sayması için.</summary>
        public Action<bool> SubMenuChanged;
        public Action Resume, Restart, MainMenu;

        public bool SubMenuOpen => (settings != null && settings.IsOpen) || (confirm != null && confirm.IsOpen);

        private bool visible, lastSub;
        private float anim;

        private void Awake()
        {
            resumeButton.onClick.AddListener(() => Resume?.Invoke());
            restartButton.onClick.AddListener(() => confirm.Show("YENİDEN BAŞLAT",
                "İlerlemen kaydedilmeyen bölüm baştan başlayacak. Emin misin?", "BAŞTAN BAŞLAT", "VAZGEÇ", () => Restart?.Invoke()));
            menuButton.onClick.AddListener(() => confirm.Show("ANA MENÜ",
                "Bölümdeki ilerlemen kaydedilmeyecek. Ana menüye dönülsün mü?", "ANA MENÜ", "VAZGEÇ", () => MainMenu?.Invoke()));
            settingsButton.onClick.AddListener(OpenSettings);
            settings.Closed = CloseSettings;
            settings.Hide(true);
            SetVisible(false, true);
        }

        private void OpenSettings()
        {
            window.gameObject.SetActive(false);
            settings.Show();
        }

        private void CloseSettings()
        {
            settings.Hide();
            window.gameObject.SetActive(true);
            Select(settingsButton);
        }

        private void Update()
        {
            var open = IsPauseOpen != null && IsPauseOpen();
            if (open != visible) SetVisible(open, false);
            if (visible)
            {
                var kb = Keyboard.current;
                if (kb != null && kb.escapeKey.wasPressedThisFrame && lastSub)
                {
                    if (confirm.IsOpen) confirm.Cancel();
                    else if (settings.IsOpen && !settings.HandleBack()) CloseSettings();
                }
                anim = Mathf.MoveTowards(anim, 1f, Time.unscaledDeltaTime / (GameSettings.ReduceMotion ? 0.05f : 0.2f));
                var e = 1f - (1f - anim) * (1f - anim);
                root.alpha = e;
                window.localScale = Vector3.one * Mathf.Lerp(0.94f, 1f, e);
            }
            var sub = visible && SubMenuOpen;
            if (sub != lastSub)
            {
                lastSub = sub;
                SubMenuChanged?.Invoke(sub);
            }
        }

        private void SetVisible(bool on, bool instant)
        {
            visible = on;
            root.gameObject.SetActive(on);
            root.interactable = root.blocksRaycasts = on;
            if (on)
            {
                anim = instant ? 1f : 0f;
                window.gameObject.SetActive(true);
                Select(resumeButton);
            }
            else
            {
                if (settings.IsOpen) settings.Hide(true);
                confirm.Cancel();
            }
        }

        private static void Select(Selectable s)
        {
            if (EventSystem.current != null && s != null) EventSystem.current.SetSelectedGameObject(s.gameObject);
        }
    }
}
