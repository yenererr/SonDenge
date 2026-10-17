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

                Text icon = FindCardText(button, "KartIkon");
                Text name = FindCardText(button, "KartAdi");
                Text description = FindCardText(button, "KartAciklama");
                Text price = FindCardText(button, "KartSatinAl/KartFiyat");
                if (icon != null)
                    icon.text = GetFurnitureIcon(selectedFurniture);
                if (name != null)
                    name.text = selectedFurniture.DisplayName;
                if (description != null)
                    description.text = GetFurnitureDescription(selectedFurniture);
                if (price != null)
                    price.text = "SATIN AL\n" + selectedFurniture.PurchasePrice + " PARA";

                button.onClick.AddListener(() => TryPurchase(selectedFurniture));
                _itemButtons.Add(button);
                _catalogItems.Add(selectedFurniture);
            }
        }

        private static Text FindCardText(Button card, string childName)
        {
            Transform child = card != null ? card.transform.Find(childName) : null;
            return child != null ? child.GetComponent<Text>() : null;
        }

        private static string GetFurnitureIcon(FurnitureData furniture)
        {
            if (furniture == null || string.IsNullOrWhiteSpace(furniture.DisplayName))
                return "?";

            string name = furniture.DisplayName;
            if (name.StartsWith("Ki", StringComparison.OrdinalIgnoreCase))
                return "Kİ";

            return name.Substring(0, 1).ToUpperInvariant();
        }

        private static string GetFurnitureDescription(FurnitureData furniture)
        {
            if (furniture == null)
                return string.Empty;

            if (furniture.Mass <= 2f)
                return "Hafif - dengeyi az etkiler.";
            if (furniture.Mass <= 4f)
                return "Orta ağırlık - dengeli seçim.";
            return "Ağır - dengeyi çok etkiler.";
        }

        private void CreateDefaultUiIfNeeded()
        {
            if (!autoCreateUi || (shopContent != null && itemListRoot != null && itemButtonPrefab != null && budgetText != null && feedbackText != null && closeButton != null))
                return;

            EnsureEventSystem();

            Canvas canvas = CreateCanvas("Level2ShopCanvas");
            GameObject panel = CreatePanel("Level2ShopPanel", canvas.transform, new Vector2(900f, 780f), new Vector2(0f, 0f), new Color(0.055f, 0.08f, 0.13f, 0.98f));
            shopContent = panel;

            CreatePanel("UstSerit", panel.transform, new Vector2(900f, 112f), new Vector2(0f, 334f), new Color(0.10f, 0.16f, 0.24f, 1f));
            CreatePanel("AltinCizgi", panel.transform, new Vector2(900f, 5f), new Vector2(0f, 276f), new Color(0.95f, 0.67f, 0.24f, 1f));
            CreateLabel("Baslik", panel.transform, "LEVEL 2 MAĞAZASI", 30, TextAnchor.UpperCenter, new Vector2(0f, -22f), new Vector2(700f, 48f), new Color(1f, 0.82f, 0.42f, 1f));
            CreateLabel("AltBaslik", panel.transform, "Hazırlık aşaması - Dengeyi koru, doğru eşyayı seç", 16, TextAnchor.UpperCenter, new Vector2(0f, -68f), new Vector2(700f, 30f), new Color(0.72f, 0.84f, 0.9f, 1f));

            GameObject budgetCard = CreatePanel("ButceKarti", panel.transform, new Vector2(250f, 58f), new Vector2(-285f, 235f), new Color(0.12f, 0.28f, 0.31f, 1f));
            budgetText = CreateLabel("Butce", budgetCard.transform, string.Empty, 20, TextAnchor.MiddleCenter, new Vector2(0f, -15f), new Vector2(230f, 42f), Color.white);
            feedbackText = CreateLabel("Bilgi", panel.transform, "Yeni eşya alabilirsin - ama her eşya dengeyi etkiler!", 16, TextAnchor.MiddleCenter, new Vector2(95f, -165f), new Vector2(510f, 50f), new Color(0.65f, 0.9f, 0.9f, 1f));

            GameObject list = new GameObject("EsyaListesi", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            list.transform.SetParent(panel.transform, false);
            RectTransform listRect = list.GetComponent<RectTransform>();
            listRect.anchorMin = new Vector2(0.5f, 0.5f);
            listRect.anchorMax = new Vector2(0.5f, 0.5f);
            listRect.pivot = new Vector2(0.5f, 0.5f);
            listRect.anchoredPosition = new Vector2(0f, -60f);
            listRect.sizeDelta = new Vector2(840f, 500f);
            GridLayoutGroup layout = list.GetComponent<GridLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);
            layout.spacing = new Vector2(16f, 16f);
            layout.cellSize = new Vector2(260f, 224f);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            ContentSizeFitter fitter = list.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemListRoot = list.transform;

            itemButtonPrefab = CreateFurnitureCard("EsyaKartSablon", panel.transform);
            itemButtonPrefab.gameObject.SetActive(false);

            closeButton = CreateButton("MagazayiKapat", panel.transform, "KAPAT", new Vector2(125f, 42f), new Vector2(365f, 342f), new Color(0.45f, 0.2f, 0.2f, 1f));

            CreateLabel("AltBilgi", panel.transform, "İpucu: Ağır eşyalar dengeyi daha çok etkiler.", 14, TextAnchor.MiddleCenter, new Vector2(0f, -368f), new Vector2(760f, 28f), new Color(0.55f, 0.68f, 0.76f, 1f));

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

        private static Button CreateFurnitureCard(string objectName, Transform parent)
        {
            GameObject cardObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            cardObject.transform.SetParent(parent, false);

            RectTransform cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardRect.pivot = new Vector2(0.5f, 0.5f);
            cardRect.sizeDelta = new Vector2(260f, 224f);

            Image cardImage = cardObject.GetComponent<Image>();
            cardImage.color = new Color(0.12f, 0.21f, 0.28f, 1f);

            Button cardButton = cardObject.GetComponent<Button>();
            ColorBlock colors = cardButton.colors;
            colors.highlightedColor = new Color(0.2f, 0.38f, 0.48f, 1f);
            colors.pressedColor = new Color(0.08f, 0.16f, 0.22f, 1f);
            colors.disabledColor = new Color(0.12f, 0.17f, 0.2f, 0.55f);
            cardButton.colors = colors;

            CreatePanel("KartGorsel", cardObject.transform, new Vector2(230f, 78f), new Vector2(0f, 63f), new Color(0.08f, 0.14f, 0.19f, 1f));
            CreateLabel("KartIkon", cardObject.transform, "?", 42, TextAnchor.MiddleCenter, new Vector2(0f, -12f), new Vector2(210f, 62f), new Color(1f, 0.78f, 0.35f, 1f));
            CreateLabel("KartAdi", cardObject.transform, "EŞYA", 18, TextAnchor.MiddleCenter, new Vector2(0f, -82f), new Vector2(230f, 28f), Color.white);
            CreateLabel("KartAciklama", cardObject.transform, "Denge bilgisi", 12, TextAnchor.MiddleCenter, new Vector2(0f, -112f), new Vector2(230f, 30f), new Color(0.68f, 0.8f, 0.86f, 1f));

            GameObject priceStrip = CreatePanel("KartSatinAl", cardObject.transform, new Vector2(230f, 38f), new Vector2(0f, -88f), new Color(0.28f, 0.48f, 0.29f, 1f));
            CreateLabel("KartFiyat", priceStrip.transform, "SATIN AL\n0 PARA", 13, TextAnchor.MiddleCenter, new Vector2(0f, -5f), new Vector2(220f, 32f), Color.white);
            return cardButton;
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
