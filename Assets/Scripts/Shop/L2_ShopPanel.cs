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
        private bool _purchasesEnabled = true;

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

        /// <summary>Yerleştirme sistemi gibi yardımcı sistemlerin mağaza mesajını güncellemesi için kullanılır.</summary>
        public void ShowFeedback(string message)
        {
            SetFeedback(message);
        }

        /// <summary>Hazırlıkta iptal edilen yerleştirme için satın alma bedelini iade eder.</summary>
        public void RefundPurchase(int amount)
        {
            _budget += Mathf.Max(0, amount);
            RefreshView();
        }

        /// <summary>Bir mobilya yerleştirilirken ikinci satın almayı engeller.</summary>
        public void SetPurchasingEnabled(bool enabled)
        {
            _purchasesEnabled = enabled;
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
            SetFeedback(furniture.DisplayName + " satın alındı.");
            OnFurniturePurchased?.Invoke(furniture);
            if (onFurniturePurchased != null)
                onFurniturePurchased.Invoke(furniture);

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
                {
                    label.text = selectedFurniture.DisplayName + "\nSATIN AL  -  " + selectedFurniture.PurchasePrice + " PARA";
                    label.fontSize = 16;
                    label.fontStyle = FontStyle.Bold;
                }

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
            GameObject panel = CreatePanel("Level2ShopPanel", canvas.transform, new Vector2(820f, 700f), new Vector2(0f, 0f), new Color(0.055f, 0.08f, 0.13f, 0.98f));
            shopContent = panel;

            CreatePanel("UstSerit", panel.transform, new Vector2(820f, 104f), new Vector2(0f, 298f), new Color(0.10f, 0.16f, 0.24f, 1f));
            CreatePanel("AltinCizgi", panel.transform, new Vector2(820f, 5f), new Vector2(0f, 245f), new Color(0.95f, 0.67f, 0.24f, 1f));
            CreateLabel("Baslik", panel.transform, "LEVEL 2 MAĞAZASI", 30, TextAnchor.UpperCenter, new Vector2(0f, -20f), new Vector2(650f, 48f), new Color(1f, 0.82f, 0.42f, 1f));
            CreateLabel("AltBaslik", panel.transform, "Hazırlık aşaması - Dengeyi koru, doğru eşyayı seç", 16, TextAnchor.UpperCenter, new Vector2(0f, -64f), new Vector2(650f, 30f), new Color(0.72f, 0.84f, 0.9f, 1f));

            GameObject budgetCard = CreatePanel("ButceKarti", panel.transform, new Vector2(250f, 62f), new Vector2(-225f, -130f), new Color(0.12f, 0.28f, 0.31f, 1f));
            budgetText = CreateLabel("Butce", budgetCard.transform, string.Empty, 20, TextAnchor.MiddleCenter, new Vector2(0f, -16f), new Vector2(230f, 44f), Color.white);
            feedbackText = CreateLabel("Bilgi", panel.transform, "Yeni eşya alabilirsin — ama her eşya dengeyi etkiler!", 16, TextAnchor.MiddleCenter, new Vector2(95f, -130f), new Vector2(445f, 62f), new Color(0.65f, 0.9f, 0.9f, 1f));

            GameObject list = new GameObject("EsyaListesi", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            list.transform.SetParent(panel.transform, false);
            RectTransform listRect = list.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.pivot = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0f, -125f);
            listRect.sizeDelta = new Vector2(700f, 410f);
            VerticalLayoutGroup layout = list.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.spacing = 9f;
            layout.childControlWidth = true;
            // Butonların RectTransform yüksekliğini koru; aksi halde layout
            // sistemi tercih edilen yüksekliği 0 hesaplayıp butonları gizler.
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            ContentSizeFitter fitter = list.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemListRoot = list.transform;

            itemButtonPrefab = CreateButton("EsyaButonSablon", panel.transform, "EŞYA", new Vector2(660f, 56f), new Vector2(0f, 0f), new Color(0.14f, 0.27f, 0.36f, 1f));
            itemButtonPrefab.gameObject.SetActive(false);

            closeButton = CreateButton("MagazayiKapat", panel.transform, "KAPAT", new Vector2(125f, 42f), new Vector2(330f, -20f), new Color(0.45f, 0.2f, 0.2f, 1f));

            CreateLabel("AltBilgi", panel.transform, "İpucu: Ağır eşyalar dengeyi daha çok etkiler.", 14, TextAnchor.MiddleCenter, new Vector2(0f, -665f), new Vector2(680f, 30f), new Color(0.55f, 0.68f, 0.76f, 1f));

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
                _itemButtons[i].interactable = _purchasesEnabled && canBuy && furniture.PurchasePrice <= _budget;
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
