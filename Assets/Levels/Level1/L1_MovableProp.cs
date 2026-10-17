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
        public float balanceMassKg;
        public Rigidbody Body { get; private set; }
        public string ItemId => itemId;
        public float Mass => balanceMassKg > 0f ? balanceMassKg :
            (Body != null ? Body.mass : GetComponent<Rigidbody>().mass);
        public int Value => value;
        public bool IsAnchored { get; private set; }
        public bool AllowPlanningSlide { get; private set; }
        public bool IsPlacementGraceActive => placementGraceActive;
        private bool placementGraceActive;
        private float placementReleaseTime;
        private CollisionDetectionMode normalDetectionMode;

        private void Awake()
        {
            Body = GetComponent<Rigidbody>();
            normalDetectionMode = Body.collisionDetectionMode;
        }

        public void SetAnchored(bool anchored)
        {
            IsAnchored = anchored;
            if (Body == null) return;
            Body.isKinematic = anchored || placementGraceActive;
            Body.collisionDetectionMode = Body.isKinematic
                ? CollisionDetectionMode.ContinuousSpeculative : normalDetectionMode;
        }

        public void BeginPlacementGrace(float seconds)
        {
            placementGraceActive = true;
            placementReleaseTime = Time.time + Mathf.Max(0f, seconds);
            AllowPlanningSlide = true;
            Body.linearVelocity = Vector3.zero;
            Body.angularVelocity = Vector3.zero;
            Body.isKinematic = true;
            Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void FixedUpdate()
        {
            if (!placementGraceActive || Time.time < placementReleaseTime) return;
            placementGraceActive = false;
            SetAnchored(IsAnchored);
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
