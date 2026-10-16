using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    /// <summary>Lets a 3D Rigidbody player use the Level1 ladder in the XY plane.</summary>
    [RequireComponent(typeof(BoxCollider))]
    public sealed class L1_LadderClimb : MonoBehaviour
    {
        public Transform bottom;
        public Transform top;
        public Transform upperLanding;
        public float climbSpeed = 2.4f;

        private Rigidbody occupant;
        private bool previousGravity;

        private void OnTriggerStay(Collider other)
        {
            var body = other.attachedRigidbody;
            if (body == null || !body.gameObject.CompareTag("Player") ||
                bottom == null || top == null || upperLanding == null) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            var direction = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0)
                - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            if (direction == 0 && occupant != body) return;

            if (occupant != body)
            {
                Release();
                occupant = body;
                previousGravity = body.useGravity;
                body.useGravity = false;
            }

            body.linearVelocity = Vector3.zero;
            if (direction == 0) return;

            var axis = (top.position - bottom.position).normalized;
            var next = body.position + axis * (direction * climbSpeed * Time.fixedDeltaTime);
            var height = Vector3.Dot(next - bottom.position, axis);
            var totalHeight = Vector3.Distance(bottom.position, top.position);
            if (height >= totalHeight && direction > 0)
            {
                body.position = upperLanding.position;
                Release();
            }
            else if (height <= 0f && direction < 0)
            {
                body.position = bottom.position;
                Release();
            }
            else
            {
                body.MovePosition(next);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == occupant) Release();
        }

        private void OnDisable() => Release();

        private void Release()
        {
            if (occupant == null) return;
            occupant.useGravity = previousGravity;
            occupant = null;
        }
    }
}
