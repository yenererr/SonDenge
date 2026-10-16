using UnityEngine;
using UnityEngine.InputSystem;
using Jam.Core;

namespace Jam.Levels.Level1
{
    public sealed class L1_PropDrag : MonoBehaviour
    {
        public Camera sceneCamera;
        public float followGain = 10f;
        public float maxSpeed = 5f;
        public float maxLift = 1.1f;
        private L1_MovableProp held;
        private L1_MovableProp hover;
        private Vector3 offset;
        private Vector3 target;
        private float startY;
        private bool previousGravity;
        private GUIStyle label;
        private void Update()
        {
            if (Time.timeScale <= 0 || Mouse.current == null) { Release(); return; }
            var mouse = Mouse.current;
            var ray = sceneCamera.ScreenPointToRay(mouse.position.ReadValue());
            hover = null;
            if (UnityEngine.Physics.Raycast(ray, out var hit, 50f)) hover = hit.collider.GetComponentInParent<L1_MovableProp>();
            if (mouse.leftButton.wasPressedThisFrame && hover != null && !hover.IsAnchored)
            {
                held = hover;
                previousGravity = held.Body.useGravity;
                held.Body.useGravity = false;
                startY = held.Body.position.y;
                var plane = new Plane(Vector3.forward, held.Body.position);
                if (plane.Raycast(ray, out var d)) offset = held.Body.position - ray.GetPoint(d);
                target = held.Body.position;
            }
            if (held != null)
            {
                var plane = new Plane(Vector3.forward, held.Body.position);
                if (plane.Raycast(ray, out var d))
                {
                    target = ray.GetPoint(d) + offset;
                    target.y = Mathf.Clamp(target.y, startY - maxLift, startY + maxLift);
                }
                if (!mouse.leftButton.isPressed) Release();
            }
        }
        private void FixedUpdate()
        {
            if (held == null) return;
            // Dynamic motion keeps wall/floor collisions active while dragging.
            held.Body.linearVelocity = Vector3.ClampMagnitude((target - held.Body.position) * followGain, maxSpeed);
            held.Body.angularVelocity *= 0.7f;
        }
        private void OnDisable() => Release();
        private void Release()
        {
            if (held == null) return;
            held.Body.useGravity = previousGravity;
            held.Body.linearVelocity = Vector3.zero;
            held = null;
        }
        private void OnGUI()
        {
            if (label == null) label = new GUIStyle(GUI.skin.box) { fontSize = 15, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(16,16,8,8) };
            GUI.Box(new Rect(22, Screen.height - 64, 700, 42), "A / D  Yürü    •    W / S  Merdiven    •    Sol fare  Eşyayı tut ve taşı    •    ESC  Duraklat", label);
            var item = held != null ? held : hover;
            if (item != null) GUI.Box(new Rect(22, Screen.height - 108, 350, 38), item.displayName + "  •  " + item.Mass + " kg", label);
        }
    }
}
