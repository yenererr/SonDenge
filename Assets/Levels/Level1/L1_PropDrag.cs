using UnityEngine;
using UnityEngine.InputSystem;
using Jam.Core;

namespace Jam.Levels.Level1
{
    public sealed class L1_PropDrag : MonoBehaviour
    {
        public Camera sceneCamera;
        public Transform houseRoot;
        public float followGain = 7f;
        public float maxSpeed = 2.8f;
        public float maxLift = 1.1f;
        public float contactGap = 0.025f;
        public float interactionReach = 1.35f;
        public bool canDrag = true;
        public L1_MovableProp Selected { get; private set; }
        public L1_MovableProp Carried => carried;
        public float CarriedLocalX => carryLocalPosition.x;
        private L1_MovableProp held;
        private L1_MovableProp carried;
        private L1_MovableProp hover;
        private L1_ResidentController resident;
        private BoxCollider heldCollider;
        private BoxCollider carriedCollider;
        private Vector3 offset;
        private Vector3 target;
        private float startY;
        private bool previousGravity;
        private bool previousKinematic;
        private bool carriedGravity;
        private bool carriedKinematic;
        private Vector3 carryLocalPosition;
        private bool usingTouch;
        private bool actionWasHeld;
        private float dropBlockedUntil;
        private GUIStyle label;
        private void Awake()
        {
            if (houseRoot != null) resident = houseRoot.GetComponentInChildren<L1_ResidentController>(true);
            if (sceneCamera == null) sceneCamera = Camera.main;
        }
        private void Update()
        {
            var actionHeld = L1_TouchControls.ActionHeld;
            var actionPressed = actionHeld && !actionWasHeld;
            actionWasHeld = actionHeld;
            if (Time.timeScale <= 0 || !canDrag) { ReleasePointer(); return; }
            var keyboard = Keyboard.current;
            if ((keyboard != null && keyboard.eKey.wasPressedThisFrame) || actionPressed)
            {
                if (carried != null) DropCarried();
                else PickUpNearest();
            }
            if (carried != null) { hover = carried; return; }
            var mouse = Mouse.current;
            var touch = Touchscreen.current != null ? Touchscreen.current.primaryTouch : null;
            var touchPressed = touch != null && touch.press.isPressed;
            var pointerIsTouch = held != null ? usingTouch : touchPressed;
            if (!pointerIsTouch && mouse == null) { ReleasePointer(); return; }
            var pointerPosition = pointerIsTouch ? touch.position.ReadValue() : mouse.position.ReadValue();
            var pressedThisFrame = pointerIsTouch ? touch.press.wasPressedThisFrame : mouse.leftButton.wasPressedThisFrame;
            var isPressed = pointerIsTouch ? touchPressed : mouse.leftButton.isPressed;
            if (held == null && pointerIsTouch && L1_TouchControls.IsControlPosition(pointerPosition))
            { hover = null; return; }
            var ray = sceneCamera.ScreenPointToRay(pointerPosition);
            hover = null;
            var hits = UnityEngine.Physics.RaycastAll(ray, 50f, ~0, QueryTriggerInteraction.Ignore);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (var hit in hits)
            {
                hover = hit.collider.GetComponentInParent<L1_MovableProp>();
                if (hover != null) break;
            }
            if (pressedThisFrame && hover != null)
            {
                Selected = hover;
                if (hover.IsAnchored || !CanReach(hover)) return;
                usingTouch = pointerIsTouch;
                held = hover;
                heldCollider = held.GetComponent<BoxCollider>();
                previousGravity = held.Body.useGravity;
                previousKinematic = held.Body.isKinematic;
                held.Body.linearVelocity = Vector3.zero;
                held.Body.angularVelocity = Vector3.zero;
                held.Body.useGravity = false;
                held.Body.isKinematic = true;
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
                    target.z = held.Body.position.z;
                    ClampToFloor();
                }
                if (!isPressed) ReleasePointer();
            }
        }
        private void FixedUpdate()
        {
            if (carried != null && resident != null && houseRoot != null)
            {
                var local = houseRoot.InverseTransformPoint(resident.transform.position);
                var halfWidth = carriedCollider != null ? carriedCollider.size.x * 0.5f : 0.3f;
                var halfHeight = carriedCollider != null ? carriedCollider.size.y * 0.5f : 0.3f;
                local.x = Mathf.Clamp(local.x + resident.Facing * (0.3f + halfWidth), -3.15f, 3.15f);
                local.y += 0.25f + halfHeight * 0.35f;
                // The camera looks from negative Z. Keep the carried model in
                // front of the resident and the floor's front framing timbers.
                local.z = -1.55f;
                carryLocalPosition = Vector3.Lerp(carryLocalPosition, local,
                    1f - Mathf.Exp(-Time.fixedDeltaTime / 0.13f));
                carried.Body.MovePosition(houseRoot.TransformPoint(carryLocalPosition));
            }
            if (held == null) return;
            var position = held.Body.position;
            var step = Vector3.ClampMagnitude((target - position) * followGain, maxSpeed) * Time.fixedDeltaTime;
            position += AllowedMove(position, new Vector3(step.x, 0f, 0f));
            position += AllowedMove(position, new Vector3(0f, step.y, 0f));
            held.Body.MovePosition(position);
        }
        private void ClampToFloor()
        {
            if (houseRoot == null || heldCollider == null) return;
            var half = Vector3.Scale(heldCollider.size * 0.5f, Abs(heldCollider.transform.lossyScale));
            var right = houseRoot.right;
            var width = Mathf.Abs(Vector3.Dot(right, heldCollider.transform.right)) * half.x
                + Mathf.Abs(Vector3.Dot(right, heldCollider.transform.up)) * half.y
                + Mathf.Abs(Vector3.Dot(right, heldCollider.transform.forward)) * half.z;
            var local = houseRoot.InverseTransformPoint(target);
            // Allow a held item to cross the floor edge and fall when released.
            local.x = Mathf.Clamp(local.x, -4.20f + width, 4.20f - width);
            target = houseRoot.TransformPoint(local);
            target.z = held.Body.position.z;
        }
        private Vector3 AllowedMove(Vector3 position, Vector3 delta)
        {
            var distance = delta.magnitude;
            if (distance < 0.00001f || heldCollider == null) return delta;
            var direction = delta / distance;
            var half = Vector3.Scale(heldCollider.size * 0.5f, Abs(heldCollider.transform.lossyScale));
            var center = position + heldCollider.transform.rotation * Vector3.Scale(heldCollider.center, heldCollider.transform.lossyScale);
            var hits = UnityEngine.Physics.BoxCastAll(center, half, direction, heldCollider.transform.rotation,
                distance + contactGap, ~0, QueryTriggerInteraction.Ignore);
            var allowed = distance;
            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(held.transform) || hit.distance < 0.001f ||
                    hit.collider.name == "Yan duvar çarpışması") continue;
                allowed = Mathf.Min(allowed, Mathf.Max(0f, hit.distance - contactGap));
            }
            return direction * allowed;
        }
        private static Vector3 Abs(Vector3 value) => new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        private void OnDisable()
        {
            ReleasePointer();
            DropCarried(true);
        }
        private void ReleasePointer()
        {
            if (held == null) return;
            held.Body.isKinematic = previousKinematic;
            held.Body.useGravity = previousGravity;
            if (!held.Body.isKinematic)
            {
                held.Body.linearVelocity = Vector3.zero;
                held.Body.angularVelocity = Vector3.zero;
            }
            held = null;
            heldCollider = null;
            usingTouch = false;
        }
        private bool CanReach(L1_MovableProp item)
        {
            if (item == null || resident == null || houseRoot == null || !item.gameObject.activeInHierarchy) return false;
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            var prop = houseRoot.InverseTransformPoint(item.transform.position);
            var box = item.GetComponent<BoxCollider>();
            var propFloor = prop.y - (box != null ? box.size.y * 0.5f : 0.3f);
            var reach = interactionReach + (box != null ? box.size.x * 0.35f : 0f);
            return Mathf.Abs(player.x - prop.x) <= reach &&
                Mathf.Abs((player.y - resident.footOffset) - propFloor) < 0.7f;
        }
        private void PickUpNearest()
        {
            if (resident == null || houseRoot == null) return;
            L1_MovableProp nearest = null;
            var best = float.MaxValue;
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            foreach (var item in houseRoot.GetComponentsInChildren<L1_MovableProp>())
            {
                if (item.IsAnchored || !CanReach(item)) continue;
                var distance = Mathf.Abs(houseRoot.InverseTransformPoint(item.transform.position).x - player.x);
                if (distance >= best) continue;
                best = distance;
                nearest = item;
            }
            if (nearest == null) return;
            ReleasePointer();
            Selected = carried = nearest;
            carriedCollider = carried.GetComponent<BoxCollider>();
            carriedGravity = carried.Body.useGravity;
            carriedKinematic = carried.Body.isKinematic;
            carried.Body.linearVelocity = Vector3.zero;
            carried.Body.angularVelocity = Vector3.zero;
            carried.Body.useGravity = false;
            carried.Body.isKinematic = true;
            if (carriedCollider != null) carriedCollider.enabled = false;
            carryLocalPosition = houseRoot.InverseTransformPoint(carried.Body.position);
            carryLocalPosition.z = -1.55f;
            carried.Body.position = houseRoot.TransformPoint(carryLocalPosition);
        }
        public bool DropCarried(bool force = false)
        {
            if (carried == null) return true;
            if (resident == null || houseRoot == null) return false;
            if (resident.IsClimbing && !force) { dropBlockedUntil = Time.unscaledTime + 1.5f; return false; }
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            var box = carriedCollider;
            var half = box != null ? Vector3.Scale(box.size * 0.5f, Abs(box.transform.lossyScale)) : Vector3.one * 0.3f;
            var floor = Mathf.Clamp(Mathf.RoundToInt((player.y - resident.footOffset + 1.15f) / 2.25f), 0, 2);
            var floorY = -1.15f + 2.25f * floor;
            var preferred = player.x + resident.Facing * (0.3f + half.x);
            var offsets = new[] { 0f, -0.4f, 0.4f, -0.8f, 0.8f, -1.2f, 1.2f };
            var found = false;
            var destination = Vector3.zero;
            foreach (var offset in offsets)
            {
                var x = preferred + offset;
                if (Mathf.Abs(x) + half.x > 3.25f) continue;
                var candidate = houseRoot.TransformPoint(new Vector3(x, floorY + half.y + 0.04f, 0.25f));
                var blocked = false;
                foreach (var hit in UnityEngine.Physics.OverlapBox(candidate, half * 0.95f,
                    box != null ? box.transform.rotation : houseRoot.rotation, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.IsChildOf(carried.transform)) continue;
                    if (hit.GetComponentInParent<L1_MovableProp>() != null) { blocked = true; break; }
                }
                if (blocked) continue;
                destination = candidate;
                found = true;
                break;
            }
            if (!found && !force) { dropBlockedUntil = Time.unscaledTime + 1.5f; return false; }
            if (!found)
                destination = houseRoot.TransformPoint(new Vector3(Mathf.Clamp(preferred, -3.0f, 3.0f),
                    floorY + half.y + 0.04f, 0.25f));
            carried.Body.position = destination;
            if (box != null) box.enabled = true;
            carried.AllowFallAtEdges(houseRoot);
            carried.Body.isKinematic = carriedKinematic;
            carried.Body.useGravity = carriedGravity;
            if (!carried.Body.isKinematic)
            {
                carried.Body.linearVelocity = Vector3.zero;
                carried.Body.angularVelocity = Vector3.zero;
            }
            carried = null;
            carriedCollider = null;
            return true;
        }
        private void OnGUI()
        {
            if (L1_TouchControls.Available || !canDrag) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(16, 16, 8, 8) };
                label.normal.textColor = L1_PaperGui.Ink;
            }
            var hintY = Selected != null ? Screen.height - 122 : Screen.height - 64;
            var hintRect = new Rect(22, hintY, 795, 42);
            L1_PaperGui.Panel(hintRect, L1_PaperGui.Parchment, L1_PaperGui.Edge);
            GUI.Label(hintRect, "A / D  Yürü    •    W / S  Merdiven    •    E  Yakındaki eşyayı al / bırak    •    Sol fare  Sürükle", label);
            var item = carried != null ? carried : held != null ? held : hover;
            if (item != null)
            {
                var itemRect = new Rect(22, hintY - 44, 440, 38);
                L1_PaperGui.Panel(itemRect, L1_PaperGui.Parchment, L1_PaperGui.Edge);
                GUI.Label(itemRect, item.displayName + "  •  " + item.Mass + " kg" +
                    (carried != null ? "  •  Taşınıyor" : CanReach(item) ? "  •  E ile al" : "  •  Yanına yaklaş"), label);
            }
            if (Time.unscaledTime < dropBlockedUntil)
            {
                var warningRect = new Rect(22, hintY - 87, 440, 38);
                L1_PaperGui.Panel(warningRect, L1_PaperGui.Parchment, new Color32(158, 58, 47, 255));
                GUI.Label(warningRect, "Bırakmak için zeminde boş bir yere geç.", label);
            }
        }
    }
}
