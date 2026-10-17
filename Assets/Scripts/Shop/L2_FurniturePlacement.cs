using System.Collections.Generic;
using Jam.Core;
using Jam.Economy;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Jam.Shop
{
    /// <summary>
    /// Mağazadan alınan mobilyayı hazırlık aşamasında zemine yerleştirir.
    /// Yeni Input System ile fare sol tıkı onay, Escape iptal olarak kullanılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class L2_FurniturePlacement : MonoBehaviour
    {
        [Header("Bağlantılar")]
        [SerializeField] private L2_ShopPanel shopPanel;
        [SerializeField] private Transform placementRoot;
        [SerializeField] private Camera placementCamera;

        [Header("Yerleştirme")]
        [SerializeField] private float placementPlaneHeight;
        [Min(0.1f)] [SerializeField] private float gridSize = 0.5f;
        [Min(1)] [SerializeField] private int maxPlacedFurniture = 40;
        [SerializeField] private bool autoCreatePlacementRoot = true;

        private readonly List<L2_PlacedFurniture> _placedFurniture = new List<L2_PlacedFurniture>();
        private L2_PlacedFurniture _preview;
        private FurnitureData _pendingFurniture;

        private void Awake()
        {
            ResolveReferences();
            EnsurePlacementRoot();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (shopPanel != null)
                shopPanel.OnFurniturePurchased += BeginPlacement;
            GameManager.OnPhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            if (shopPanel != null)
                shopPanel.OnFurniturePurchased -= BeginPlacement;
            GameManager.OnPhaseChanged -= OnPhaseChanged;
        }

        private void Update()
        {
            if (_preview == null || _pendingFurniture == null)
                return;

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPlacement();
                return;
            }

            if (TryGetPointerPosition(out Vector3 pointerPosition))
                _preview.transform.position = pointerPosition;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                ConfirmPlacement();
            }
        }

        private void LateUpdate()
        {
            if (_preview != null && shopPanel != null)
                shopPanel.ShowFeedback(_pendingFurniture.DisplayName + " seçildi. Zemine tıkla.");
        }

        private void BeginPlacement(FurnitureData furniture)
        {
            if (furniture == null || !IsPreparationPhase())
                return;

            if (_placedFurniture.Count >= maxPlacedFurniture)
            {
                shopPanel.RefundPurchase(furniture.PurchasePrice);
                shopPanel.ShowFeedback("Level2 mobilya alanı dolu.");
                return;
            }

            if (furniture.ModelPrefab == null)
            {
                shopPanel.RefundPurchase(furniture.PurchasePrice);
                shopPanel.ShowFeedback(furniture.DisplayName + " için model prefabı atanmadı.");
                return;
            }

            if (_preview != null)
                ConfirmPlacement();

            _pendingFurniture = furniture;
            // Unity 6'da bazı prefab varlıkları generic Instantiate<GameObject>
            // çağrısında geçersiz cast üretebiliyor. Non-generic API ile güvenli
            // şekilde oluşturup sonucu açıkça kontrol ediyoruz.
            UnityEngine.Object spawned = UnityEngine.Object.Instantiate((UnityEngine.Object)furniture.ModelPrefab, placementRoot, false);
            GameObject instance = spawned as GameObject;
            if (instance == null)
            {
                _pendingFurniture = null;
                shopPanel.RefundPurchase(furniture.PurchasePrice);
                shopPanel.ShowFeedback(furniture.DisplayName + " modeli oluşturulamadı.");
                return;
            }

            _preview = instance.GetComponent<L2_PlacedFurniture>();
            if (_preview == null)
                _preview = instance.AddComponent<L2_PlacedFurniture>();

            _preview.Initialize(furniture);
            _preview.transform.position = GetDefaultPlacementPosition();
            shopPanel.SetPurchasingEnabled(false);
            shopPanel.Close();
        }

        private void ConfirmPlacement()
        {
            if (_preview == null || _pendingFurniture == null)
                return;

            _preview.SetAnchored(true);
            _placedFurniture.Add(_preview);
            _preview = null;
            _pendingFurniture = null;
            shopPanel.SetPurchasingEnabled(true);
        }

        private void CancelPlacement()
        {
            if (_preview == null || _pendingFurniture == null)
                return;

            Destroy(_preview.gameObject);
            shopPanel.RefundPurchase(_pendingFurniture.PurchasePrice);
            shopPanel.ShowFeedback("Yerleştirme iptal edildi; paran iade edildi.");
            _preview = null;
            _pendingFurniture = null;
            shopPanel.SetPurchasingEnabled(true);
        }

        private bool TryGetPointerPosition(out Vector3 position)
        {
            position = GetDefaultPlacementPosition();
            if (placementCamera == null || Mouse.current == null)
                return false;

            Ray ray = placementCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            Plane placementPlane = new Plane(Vector3.up, new Vector3(0f, placementPlaneHeight, 0f));
            if (!placementPlane.Raycast(ray, out float distance))
                return false;

            Vector3 hit = ray.GetPoint(distance);
            position = SnapToGrid(new Vector3(hit.x, placementPlaneHeight, hit.z));
            return true;
        }

        private Vector3 GetDefaultPlacementPosition()
        {
            Vector3 rootPosition = placementRoot != null ? placementRoot.position : Vector3.zero;
            return SnapToGrid(new Vector3(rootPosition.x, placementPlaneHeight, rootPosition.z));
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float safeGrid = Mathf.Max(0.1f, gridSize);
            position.x = Mathf.Round(position.x / safeGrid) * safeGrid;
            position.z = Mathf.Round(position.z / safeGrid) * safeGrid;
            position.y = placementPlaneHeight;
            return position;
        }

        private void ResolveReferences()
        {
            if (shopPanel == null)
                shopPanel = GetComponent<L2_ShopPanel>();
            if (shopPanel == null)
                shopPanel = FindFirstObjectByType<L2_ShopPanel>();
            if (placementCamera == null)
                placementCamera = Camera.main;
        }

        private void EnsurePlacementRoot()
        {
            if (placementRoot != null || !autoCreatePlacementRoot)
                return;

            GameObject root = new GameObject("Level2MobilyaYerlesimAlani");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = Vector3.zero;
            placementRoot = root.transform;
        }

        private void OnPhaseChanged(GamePhase phase)
        {
            if (phase != GamePhase.Preparation && _preview != null)
                CancelPlacement();
        }

        private static bool IsPreparationPhase()
        {
            return GameManager.Instance == null || GameManager.Instance.Phase == GamePhase.Preparation;
        }
    }
}
