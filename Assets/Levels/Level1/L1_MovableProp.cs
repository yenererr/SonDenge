using Jam.Core;
using UnityEngine;

namespace Jam.Levels.Level1
{
    [RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
    public sealed class L1_MovableProp : MonoBehaviour, IFurnitureItem
    {
        public string displayName;
        public string itemId;
        public int value = 10;
        public Rigidbody Body { get; private set; }
        public string ItemId => itemId;
        public float Mass => Body != null ? Body.mass : GetComponent<Rigidbody>().mass;
        public int Value => value;
        public bool IsAnchored { get; private set; }
        private void Awake() => Body = GetComponent<Rigidbody>();
        public void SetAnchored(bool anchored)
        {
            IsAnchored = anchored;
            if (Body != null) Body.isKinematic = anchored;
        }
    }
}
