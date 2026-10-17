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

        public void AllowFallAtEdges(Transform houseRoot)
        {
            var box = GetComponent<BoxCollider>();
            if (box == null || houseRoot == null || !box.enabled) return;
            // Side walls stop the resident, while loose furniture may pass through
            // them and lose support at either end of the floor.
            foreach (var wall in houseRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                if (wall.name != "Yan duvar çarpışması" || !wall.gameObject.activeInHierarchy) continue;
                Physics.IgnoreCollision(box, wall, true);
            }
        }
    }
}
