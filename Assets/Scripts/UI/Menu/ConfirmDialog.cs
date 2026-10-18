using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Jam.UI.Menu
{
    /// <summary>
    /// Ahşap/parşömen onay penceresi. ESC = vazgeç. İsteğe bağlı geri sayım: süre bitince vazgeç çalışır
    /// (ör. görüntü ayarı onaylanmazsa geri alınır).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ConfirmDialog : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;
        [SerializeField] private TMP_Text yesLabel;
        [SerializeField] private TMP_Text noLabel;
        [SerializeField] private RectTransform window;

        public bool IsOpen { get; private set; }

        private Action onYes, onNo;
        private CanvasGroup group;
        private GameObject previousSelection;
        private Coroutine countdown;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            yesButton.onClick.AddListener(() => Close(true));
            noButton.onClick.AddListener(() => Close(false));
        }

        public void Show(string title, string message, string yes, string no, Action yesAction, Action noAction = null,
            float timeoutSeconds = 0f)
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            onYes = yesAction;
            onNo = noAction;
            titleText.text = title;
            messageText.text = message;
            yesLabel.text = yes;
            noLabel.text = no;
            noButton.gameObject.SetActive(!string.IsNullOrEmpty(no));
            previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            IsOpen = true;
            group.alpha = 1f;
            group.interactable = group.blocksRaycasts = true;
            if (window != null) StartCoroutine(Pop());
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject((noButton.gameObject.activeSelf ? noButton : yesButton).gameObject);
            if (countdown != null) StopCoroutine(countdown);
            countdown = timeoutSeconds > 0f ? StartCoroutine(Countdown(message, timeoutSeconds)) : null;
        }

        public void Cancel()
        {
            if (IsOpen) Close(false);
        }

        private void Close(bool accepted)
        {
            if (!IsOpen) return;
            var action = accepted ? onYes : onNo;
            onYes = onNo = null;
            if (countdown != null) { StopCoroutine(countdown); countdown = null; }
            Hide();
            if (EventSystem.current != null && previousSelection != null && previousSelection.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(previousSelection);
            action?.Invoke();
        }

        private void Hide()
        {
            IsOpen = false;
            group.alpha = 0f;
            group.interactable = group.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private IEnumerator Pop()
        {
            var d = GameSettings.ReduceMotion ? 0.01f : 0.18f;
            for (var t = 0f; t < d; t += Time.unscaledDeltaTime)
            {
                var k = t / d;
                window.localScale = Vector3.one * Mathf.Lerp(0.9f, 1f, 1f - (1f - k) * (1f - k));
                yield return null;
            }
            window.localScale = Vector3.one;
        }

        private IEnumerator Countdown(string message, float seconds)
        {
            for (var left = seconds; left > 0f; left -= Time.unscaledDeltaTime)
            {
                messageText.text = message + "\n<size=80%>(" + Mathf.CeilToInt(left) + " sn içinde geri alınacak)</size>";
                yield return null;
            }
            countdown = null;
            Close(false);
        }
    }
}
