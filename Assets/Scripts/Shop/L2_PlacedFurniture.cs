using Jam.Core;
using Jam.Economy;
using UnityEngine;

namespace Jam.Shop
{
    /// <summary>
    /// Level2'ye yerleştirilen mobilyanın ortak fizik verisini taşır.
    /// Hazırlık aşamasında kinematic tutulur; fizik sistemi daha sonra serbest bırakabilir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class L2_PlacedFurniture : MonoBehaviour, IFurnitureItem
    {
        private FurnitureData _data;
        private Rigidbody _body;

        public string ItemId => _data != null ? _data.ItemId : name;
        public float Mass => _data != null ? _data.Mass : (_body != null ? _body.mass : 1f);
        public int Value => _data != null ? _data.Value : 0;
        public bool IsAnchored { get; private set; }

        public void Initialize(FurnitureData furniture)
        {
            _data = furniture;
            EnsurePhysicsComponents();

            if (_data != null)
            {
                gameObject.name = "Mobilya_" + _data.ItemId;
                _body.mass = Mathf.Max(0.01f, _data.Mass);
            }

            SetAnchored(true);
        }

        public void SetAnchored(bool anchored)
        {
            IsAnchored = anchored;
            if (_body == null)
                return;

            _body.isKinematic = true;
            _body.useGravity = false;
        }

        private void EnsurePhysicsComponents()
        {
            _body = GetComponent<Rigidbody>();
            if (_body == null)
                _body = gameObject.AddComponent<Rigidbody>();

            if (GetComponentInChildren<Collider>() == null)
                gameObject.AddComponent<BoxCollider>();
        }
    }
}
