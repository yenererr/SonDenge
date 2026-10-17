using UnityEngine;

namespace Jam.Economy
{
    /// <summary>
    /// Mağaza ve fizik sistemlerinin paylaştığı tek mobilya veri kaynağı.
    /// Model referansı, Lead'in ortak prefabları hazır olduğunda Inspector'dan atanır.
    /// </summary>
    [CreateAssetMenu(fileName = "FurnitureData", menuName = "Son Denge/Mobilya Verisi")]
    public sealed class FurnitureData : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private string itemId;
        [SerializeField] private string displayName;

        [Header("Oynanış")]
        [Min(0f)] [SerializeField] private float mass = 1f;
        [Min(0)] [SerializeField] private int value = 10;
        [Min(0)] [SerializeField] private int anchoringCost = 5;
        [Min(0)] [SerializeField] private int purchasePrice = 10;
        [Tooltip("Eşyanın başlangıç dayanıklılığı. 0-100 aralığında tutulur.")]
        [Range(0f, 100f)] [SerializeField] private float durability = 100f;

        [Header("Görsel")]
        [SerializeField] private GameObject modelPrefab;

        public string ItemId => itemId;
        public string DisplayName => displayName;
        public float Mass => mass;
        public int Value => value;
        public int AnchoringCost => anchoringCost;
        public int PurchasePrice => purchasePrice;
        public float Durability => durability;
        public GameObject ModelPrefab => modelPrefab;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(itemId))
                itemId = name.ToLowerInvariant().Replace(" ", "-");

            if (string.IsNullOrWhiteSpace(displayName))
                displayName = name;
        }
    }
}
