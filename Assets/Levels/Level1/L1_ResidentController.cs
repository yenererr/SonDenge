using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class L1_ResidentController : MonoBehaviour
    {
        public float walkSpeed = 2.2f;
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
        public int Facing => facing;
        public bool IsClimbing => route != null;
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
                    var entry = vertical > 0 ? candidate.bottom.position : candidate.top.position;
                    if (Mathf.Abs(body.position.x - entry.x) < ladderReach && Mathf.Abs(body.position.y - entry.y) < 0.22f)
                    {
                        route = candidate;
                        body.useGravity = false;
                        body.linearVelocity = Vector3.zero;
                        UnityEngine.Physics.IgnoreCollision(capsule, route.upperFloor, true);
                        route.SetOpen(true);
                        body.position = entry;
                        break;
                    }
                }
            }
            if (route != null)
            {
                body.linearVelocity = Vector3.zero;
                var next = body.position + Vector3.up * (vertical * climbSpeed * Time.fixedDeltaTime);
                if (vertical > 0 && next.y >= route.top.position.y)
                {
                    body.position = route.landing.position;
                    EndClimb();
                }
                else if (vertical < 0 && next.y <= route.bottom.position.y)
                {
                    body.position = route.bottom.position;
                    EndClimb();
                }
                else body.MovePosition(next);
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
        private void EndClimb()
        {
            if (body != null) body.useGravity = true;
            if (route == null) return;
            UnityEngine.Physics.IgnoreCollision(capsule, route.upperFloor, false);
            route.SetOpen(false);
            route = null;
        }
    }
}
