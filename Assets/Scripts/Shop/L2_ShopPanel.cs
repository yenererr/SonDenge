using System;
using System.Collections.Generic;
using Jam.Core;
using Jam.Economy;
using UnityEngine;
using UnityEngine.Events;
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

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            BuildCatalogButtons();
            RefreshView();
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
                button.name = "Buton_" + selectedFurniture.ItemId;

                Text label = button.GetComponentInChildren<Text>(true);
                if (label != null)
                    label.text = selectedFurniture.DisplayName + "\n" + selectedFurniture.PurchasePrice + " PARA";

                button.onClick.AddListener(() => TryPurchase(selectedFurniture));
                _itemButtons.Add(button);
                _catalogItems.Add(selectedFurniture);
            }
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
