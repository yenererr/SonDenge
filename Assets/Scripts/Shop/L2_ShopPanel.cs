using System;
using System.Collections.Generic;
using Jam.Core;
using Jam.Economy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Jam.Shop
{
    /// <summary>
    /// Level2 mağaza paneli. Hazırlık fazında mobilya satın almayı yönetir.
    /// Satın alınan veri, event üzerinden mobilya yerleştirme sistemine aktarılır.
    /// </summary>
    public sealed class L2_ShopPanel : MonoBehaviour
    {
        [Serializable]
        public sealed class FurniturePurchasedEvent : UnityEvent<FurnitureData> { }

        [Header("Katalog")]
        [SerializeField] private FurnitureData[] catalog;
        [Min(0)] [SerializeField] private int startingBudget = 100;

        [Header("UI Referansları")]
        [SerializeField] private bool autoCreateUi = true;
        [SerializeField] private bool showOnStart = true;
        [SerializeField] private GameObject shopContent;
        [SerializeField] private Transform itemListRoot;
        [SerializeField] private Button itemButtonPrefab;
        [SerializeField] private Text budgetText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private Button closeButton;

        [Header("Entegrasyon")]
        [SerializeField] private FurniturePurchasedEvent onFurniturePurchased;

        private readonly List<Button> _itemButtons = new List<Button>();
        private readonly List<FurnitureData> _catalogItems = new List<FurnitureData>();
        private int _budget;

        /// <summary>Mağazadaki mevcut para. Lead ekonomi sistemiyle senkronlanabilir.</summary>
        public int Budget => _budget;

        /// <summary>Satın alma sonrası yerleştirme sisteminin dinleyebileceği event.</summary>
        public event Action<FurnitureData> OnFurniturePurchased;

        private void Awake()
        {
            _budget = startingBudget;

            CreateDefaultUiIfNeeded();

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            BuildCatalogButtons();
            RefreshView();

            if (!showOnStart)
                Close();
        }

        private void OnEnable()
        {
            GameManager.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            GameManager.OnPhaseChanged -= OnPhaseChanged;
        }

        /// <summary>
        /// Mağazayı açar. Mağaza yalnızca hazırlık fazında kullanılabilir.
        /// </summary>
        public void Open()
        {
            if (!IsPreparationPhase())
            {
                SetFeedback("Mağaza yalnızca hazırlık aşamasında kullanılabilir.");
                return;
            }

            if (shopContent != null)
                shopContent.SetActive(true);

            RefreshView();
        }

        public void Close()
        {
            if (shopContent != null)
                shopContent.SetActive(false);
        }

        public void Toggle()
        {
            if (shopContent != null && shopContent.activeSelf)
                Close();
            else
                Open();
        }

        /// <summary>
        /// Lead'in ortak ekonomi sistemi hazır olduğunda başlangıç bütçesini aktarabilmesi için kullanılır.
        /// </summary>
        public void SetBudget(int budget)
        {
            _budget = Mathf.Max(0, budget);
            RefreshView();
        }

        /// <summary>
        /// Bir mobilya satın alır; başarı durumunu döndürür.
        /// </summary>
        public bool TryPurchase(FurnitureData furniture)
        {
            if (furniture == null)
            {
                SetFeedback("Satın alınacak eşya bulunamadı.");
                return false;
            }

            if (!IsPreparationPhase())
            {
                SetFeedback("Eşya yalnızca hazırlık aşamasında satın alınabilir.");
                return false;
            }

            if (furniture.PurchasePrice > _budget)
            {
                SetFeedback("Bütçen bu eşya için yeterli değil.");
                RefreshView();
                return false;
            }

            _budget -= furniture.PurchasePrice;
            OnFurniturePurchased?.Invoke(furniture);
            if (onFurniturePurchased != null)
                onFurniturePurchased.Invoke(furniture);

            SetFeedback(furniture.DisplayName + " satın alındı.");
            RefreshView();
            return true;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.Preparation)
                Close();

            RefreshView();
        }

        private void BuildCatalogButtons()
        {
            if (itemListRoot == null || itemButtonPrefab == null || catalog == null)
                return;

            for (int i = itemListRoot.childCount - 1; i >= 0; i--)
                Destroy(itemListRoot.GetChild(i).gameObject);

            _itemButtons.Clear();
            _catalogItems.Clear();

            foreach (FurnitureData furniture in catalog)
            {
                if (furniture == null)
                    continue;

                FurnitureData selectedFurniture = furniture;
                Button button = Instantiate(itemButtonPrefab, itemListRoot);
                button.gameObject.SetActive(true);
                button.name = "Buton_" + selectedFurniture.ItemId;

                Text label = button.GetComponentInChildren<Text>(true);
                if (label != null)
                    label.text = selectedFurniture.DisplayName + "\n" + selectedFurniture.PurchasePrice + " PARA";

                button.onClick.AddListener(() => TryPurchase(selectedFurniture));
                _itemButtons.Add(button);
                _catalogItems.Add(selectedFurniture);
            }
        }

        private void CreateDefaultUiIfNeeded()
        {
            if (!autoCreateUi || (shopContent != null && itemListRoot != null && itemButtonPrefab != null && budgetText != null && feedbackText != null && closeButton != null))
                return;

            EnsureEventSystem();

            Canvas canvas = CreateCanvas("Level2ShopCanvas");
            GameObject panel = CreatePanel("Level2ShopPanel", canvas.transform, new Vector2(760f, 620f), new Vector2(0f, 30f), new Color(0.08f, 0.12f, 0.18f, 0.96f));
            shopContent = panel;

            CreateLabel("Baslik", panel.transform, "LEVEL 2 MAĞAZASI", 30, TextAnchor.UpperCenter, new Vector2(0f, -32f), new Vector2(680f, 60f), new Color(1f, 0.82f, 0.42f, 1f));
            budgetText = CreateLabel("Butce", panel.transform, string.Empty, 22, TextAnchor.UpperCenter, new Vector2(0f, -90f), new Vector2(680f, 42f), Color.white);
            feedbackText = CreateLabel("Bilgi", panel.transform, "Yeni eşya alabilirsin — ama her eşya dengeyi etkiler!", 18, TextAnchor.UpperCenter, new Vector2(0f, -132f), new Vector2(680f, 48f), new Color(0.65f, 0.9f, 0.9f, 1f));

            GameObject list = new GameObject("EsyaListesi", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            list.transform.SetParent(panel.transform, false);
            RectTransform listRect = list.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.pivot = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0f, -45f);
            listRect.sizeDelta = new Vector2(650f, 350f);
            VerticalLayoutGroup layout = list.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 8, 8);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            // Butonların RectTransform yüksekliğini koru; aksi halde layout
            // sistemi tercih edilen yüksekliği 0 hesaplayıp butonları gizler.
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = list.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemListRoot = list.transform;

            itemButtonPrefab = CreateButton("EsyaButonSablon", panel.transform, "EŞYA", new Vector2(620f, 54f), new Vector2(0f, 0f), new Color(0.19f, 0.3f, 0.38f, 1f));
            itemButtonPrefab.gameObject.SetActive(false);

            closeButton = CreateButton("MagazayiKapat", panel.transform, "KAPAT", new Vector2(120f, 42f), new Vector2(300f, -32f), new Color(0.45f, 0.2f, 0.2f, 1f));

            Button openButton = CreateButton("MagazayiAc", canvas.transform, "MAĞAZA", new Vector2(220f, 60f), new Vector2(-24f, 28f), new Color(0.75f, 0.46f, 0.16f, 1f));
            RectTransform openRect = openButton.GetComponent<RectTransform>();
            openRect.anchorMin = new Vector2(1f, 0f);
            openRect.anchorMax = new Vector2(1f, 0f);
            openRect.pivot = new Vector2(1f, 0f);
            openButton.onClick.AddListener(Open);
        }

        private static Canvas CreateCanvas(string objectName)
        {
            GameObject canvasObject = new GameObject(objectName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static GameObject CreatePanel(string objectName, Transform parent, Vector2 size, Vector2 position, Color color)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateLabel(string objectName, Transform parent, string content, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size, Color color)
        {
            GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = content;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }

        private static Button CreateButton(string objectName, Transform parent, string content, Vector2 size, Vector2 position, Color color)
        {
            GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = buttonObject.GetComponent<Image>();
            image.color = color;
            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            button.colors = colors;
            Text label = CreateLabel("Yazi", buttonObject.transform, content, 18, TextAnchor.MiddleCenter, Vector2.zero, size - new Vector2(18f, 8f), Color.white);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = new Vector2(-18f, -8f);
            return button;
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            GameObject eventSystem = new GameObject("Level2EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(null);
        }

        private void RefreshView()
        {
            if (budgetText != null)
                budgetText.text = "BÜTÇE: " + _budget + " PARA";

            bool canBuy = IsPreparationPhase();
            for (int i = 0; i < _itemButtons.Count && i < _catalogItems.Count; i++)
            {
                FurnitureData furniture = _catalogItems[i];
                _itemButtons[i].interactable = canBuy && furniture.PurchasePrice <= _budget;
            }
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }

        private static bool IsPreparationPhase()
        {
            return GameManager.Instance == null || GameManager.Instance.Phase == GamePhase.Preparation;
        }
    }
}
