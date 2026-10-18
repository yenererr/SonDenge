using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class L1_ResidentController : MonoBehaviour
    {
        public float walkSpeed = 2.73f; // panel 260 px/sn * 0.0105 m/px
        public float climbSpeed = 1.6f;
        public float ladderReach = 0.52f;
        public float footOffset = 0.43f;
        public L1_ClimbRoute[] ladders;
        public Transform visual;
        public Transform leftFoot;
        public Transform rightFoot;
        public Vector3 spawnPosition;
        private Rigidbody body;
        private CapsuleCollider capsule;
        private L1_ClimbRoute route;
        private float horizontal;
        private float vertical;
        private float walkTime;
        private int facing = 1;
        private CollisionDetectionMode climbDetection = CollisionDetectionMode.Discrete;
        // The visible ladder is wider than the character capsule in the 3D house.
        // Keep a forgiving minimum so small prefab/physics offsets do not prevent entry.
        private const float MinimumLadderReach = 0.82f;
        private const float LadderEntryHeightTolerance = 0.36f;
        public int Facing => facing;
        public bool IsClimbing => route != null;
        [Tooltip("Saklanırken (Çök–Kapan–Tutun) görselin dikey ölçeği.")]
        public float hideSquash = 0.62f;
        public bool IsHiding { get; private set; }
        private Vector3 visualScale = Vector3.one;
        private Vector3 visualPosition;

        /// <summary>Depremde çömelip başını korur; hareket ve tırmanma durur.</summary>
        public void SetHiding(bool hide)
        {
            if (hide == IsHiding) return;
            if (hide && route != null) return;
            IsHiding = hide;
            if (visual == null) return;
            if (hide)
            {
                visualScale = visual.localScale;
                visualPosition = visual.localPosition;
                visual.localScale = new Vector3(visualScale.x * 1.08f, visualScale.y * hideSquash, visualScale.z);
                visual.localPosition = visualPosition + Vector3.down * (footOffset * (1f - hideSquash));
            }
            else
            {
                visual.localScale = visualScale;
                visual.localPosition = visualPosition;
            }
        }
        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();
        }
        private void Update()
        {
            var k = Keyboard.current;
            horizontal = k == null ? 0 : (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0)
                - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            vertical = k == null ? 0 : (k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0)
                - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0);
            horizontal = Mathf.Clamp(horizontal + L1_TouchControls.Horizontal, -1f, 1f);
            vertical = Mathf.Clamp(vertical + L1_TouchControls.Vertical, -1f, 1f);
            if (IsHiding) { horizontal = 0f; vertical = 0f; }
            if (horizontal > 0.01f) facing = 1;
            else if (horizontal < -0.01f) facing = -1;
            if (Time.timeScale <= 0f) return;
            walkTime += Time.deltaTime * (route == null ? Mathf.Abs(horizontal) : Mathf.Abs(vertical)) * 12f;
            if (leftFoot != null) leftFoot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(walkTime) * 20f);
            if (rightFoot != null) rightFoot.localRotation = Quaternion.Euler(0, 0, -Mathf.Sin(walkTime) * 20f);
            if (visual != null && horizontal != 0 && route == null)
                visual.localRotation = Quaternion.Euler(0, horizontal > 0 ? -16f : 16f, 0);
        }
        private void FixedUpdate()
        {
            if (body.position.y < -5f) { EndClimb(); body.position = spawnPosition; body.linearVelocity = Vector3.zero; }
            if (route == null && vertical != 0)
            {
                foreach (var candidate in ladders)
                {
                    if (candidate == null || !candidate.gameObject.activeInHierarchy) continue;
                    if (candidate.bottom == null || candidate.top == null || candidate.landing == null) continue;
                    var entry = vertical > 0 ? candidate.bottom.position : candidate.top.position;
                    var reach = Mathf.Max(ladderReach, MinimumLadderReach);
                    // Ev eğilebildiği için mesafe evin kendi ekseninde ölçülür.
                    var frame = candidate.transform.parent != null ? candidate.transform.parent : candidate.transform;
                    var delta = frame.InverseTransformPoint(body.position) - frame.InverseTransformPoint(entry);
                    if (Mathf.Abs(delta.x) < reach && Mathf.Abs(delta.y) < LadderEntryHeightTolerance)
                    {
                        route = candidate;
                        body.useGravity = false;
                        body.linearVelocity = Vector3.zero;
                        // Tırmanırken eşyalara takılmasın: kinematik ilerle.
                        climbDetection = body.collisionDetectionMode;
                        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        body.isKinematic = true;
                        if (route.upperFloor != null)
                            UnityEngine.Physics.IgnoreCollision(capsule, route.upperFloor, true);
                        route.SetOpen(true);
                        body.position = entry;
                        break;
                    }
                }
            }
            if (route != null)
            {
                // Merdivenin kendi ekseni boyunca (ev eğikken de) tırman.
                var bottomPos = route.bottom.position;
                var axis = route.top.position - bottomPos;
                var length = axis.magnitude;
                axis = length > 0.001f ? axis / length : Vector3.up;
                var along = Vector3.Dot(body.position - bottomPos, axis);
                var nextAlong = along + vertical * climbSpeed * Time.fixedDeltaTime;
                if (vertical > 0 && nextAlong >= length)
                {
                    var landing = FreeLanding(route);
                    body.position = landing;
                    transform.position = landing;
                    EndClimb();
                }
                else if (vertical < 0 && nextAlong <= 0f)
                {
                    body.position = bottomPos;
                    transform.position = bottomPos;
                    EndClimb();
                }
                else
                {
                    // Ev pivotu her fizik adımında döndüğü için MovePosition ezilir; konumu doğrudan ata.
                    var next = bottomPos + axis * Mathf.Clamp(nextAlong, 0f, length);
                    body.position = next;
                    transform.position = next;
                }
            }
            else
            {
                // The house rotates around its balance pivot. Walk along its floor,
                // otherwise world-horizontal velocity drives the capsule into uphill boards.
                var floorRight = transform.parent != null ? transform.parent.right : Vector3.right;
                var floorUp = transform.parent != null ? transform.parent.up : Vector3.up;
                var normalVelocity = Vector3.Project(body.linearVelocity, floorUp);
                var walkVelocity = floorRight * (horizontal * walkSpeed) + normalVelocity;
                walkVelocity.z = 0f;
                body.linearVelocity = walkVelocity;
            }
        }
        private void OnDisable() => EndClimb();

        /// <summary>Üst kattaki iniş noktası bir eşyayla doluysa yakındaki boş bir yere iner.</summary>
        private Vector3 FreeLanding(L1_ClimbRoute ladder)
        {
            var landing = ladder.landing.position;
            var frame = ladder.transform.parent != null ? ladder.transform.parent : ladder.transform;
            var radius = capsule != null ? capsule.radius * 0.9f : 0.2f;
            var half = capsule != null ? Mathf.Max(0f, capsule.height * 0.5f - capsule.radius) : 0.3f;
            foreach (var dx in new[] { 0f, 0.5f, -0.5f, 0.9f, -0.9f, 1.3f, -1.3f, 1.7f })
            {
                var candidate = landing + frame.right * dx;
                var blocked = false;
                foreach (var hit in UnityEngine.Physics.OverlapCapsule(candidate + frame.up * half, candidate - frame.up * half,
                    radius, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.attachedRigidbody == body || hit.transform.IsChildOf(transform)) continue;
                    if (hit.attachedRigidbody != null) { blocked = true; break; }
                }
                if (!blocked) return candidate;
            }
            return landing;
        }
        private void EndClimb()
        {
            if (body != null) body.useGravity = true;
            if (route == null) return;
            if (body != null) { body.isKinematic = false; body.collisionDetectionMode = climbDetection; }
            if (route.upperFloor != null)
                UnityEngine.Physics.IgnoreCollision(capsule, route.upperFloor, false);
            route.SetOpen(false);
            route = null;
        }
    }
}
