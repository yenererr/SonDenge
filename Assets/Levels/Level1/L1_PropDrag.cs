using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Jam.Core;

namespace Jam.Levels.Level1
{
    public sealed class L1_PropDrag : MonoBehaviour
    {
        public Camera sceneCamera;
        public Transform houseRoot;
        public float followGain = 18f;
        public float maxSpeed = 8f;
        [Tooltip("Fare imleci altındaki / seçili eşyanın parlaklık tonu.")]
        public Color hoverTint = new Color(1.25f, 1.12f, 0.85f, 1f);
        public Color selectedTint = new Color(1.12f, 1.22f, 1.15f, 1f);
        private Renderer[] tintedRenderers;
        private L1_MovableProp tintedItem;
        private Color tintedColor;
        private MaterialPropertyBlock tintBlock;
        [Tooltip("Fareyle sürüklemek için karakterin eşyaya yakın olması gereksin mi? Level 1'de kapalı.")]
        public bool mouseRequiresReach = false;
        [Tooltip("Karakterin E ile eşyayı eline alıp taşıması. Kapalıyken eşyalar yalnız fareyle sürüklenir.")]
        public bool allowCarry = false;
        [Tooltip("Açıkken eşya fareyle her yöne sürüklenir, katlar arasında taşınabilir ve bırakınca en yakın kata oturur.")]
        public bool freeDrag = true;
        [Tooltip("Odanın iç yarı genişliği (m); L1_ProjectGameController.roomHalfWidth ile aynı olmalı.")]
        public float roomHalfWidth = 4.3f;
        public float contactGap = 0.025f;
        public float interactionReach = 1.35f;
        public bool canDrag = true;
        [Tooltip("Açıksa E yalnız 'carryable' işaretli küçük eşyaları alır.")]
        public bool carryOnlyMarked = true;
        /// <summary>E basıldığında önce bu çağrılır; true dönerse (ör. yakında görev düğmesi var) taşıma yapılmaz.</summary>
        public System.Func<bool> interactOverride;
        [Tooltip("Eski IMGUI kontrol ipucu çubuğu. uGUI HUD varken kapalı.")]
        public bool showLegacyHint = false;
        public L1_MovableProp Selected { get; private set; }
        public event System.Action ManipulationStarted;
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
        private Vector3 startLocal;
        private Vector3 startWorld;
        private bool previousGravity;
        private bool previousKinematic;
        private CollisionDetectionMode previousDetection;
        private bool carriedGravity;
        private bool carriedKinematic;
        private Vector3 carryLocalPosition;
        private bool usingTouch;
        private bool heldManipulationNotified;
        private bool actionWasHeld;
        private float dropBlockedUntil;
        private GUIStyle label;
        // Taşıma sırasında kuyruğa alınmış MovePosition bırakma konumunu ezmesin diye
        // bırakılan eşya birkaç fizik adımı boyunca hedefte tutulur.
        private Rigidbody settleBody;
        private Vector3 settlePosition;
        private int settleSteps;

        private void SettleDropped()
        {
            if (settleBody == null || settleSteps <= 0) return;
            settleBody.position = settlePosition;
            settleBody.transform.position = settlePosition;
            settleBody.linearVelocity = Vector3.zero;
            settleSteps--;
            if (settleSteps <= 0) settleBody = null;
        }
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
            if (allowCarry && ((keyboard != null && keyboard.eKey.wasPressedThisFrame) || actionPressed))
            {
                if (carried != null) DropCarried();
                else if (interactOverride == null || !interactOverride()) PickUpNearest();
            }
            if (carried != null) { hover = carried; return; }
            var mouse = Mouse.current;
            var touch = Touchscreen.current != null ? Touchscreen.current.primaryTouch : null;
            var touchPressed = touch != null && touch.press.isPressed;
            var pointerIsTouch = held != null ? usingTouch : touchPressed;
            if (!pointerIsTouch && mouse == null && !testPointer) { ReleasePointer(); return; }
            Vector2 pointerPosition;
            bool pressedThisFrame, isPressed, cancel;
            if (testPointer)
            {
                // Editor/MCP testleri için sanal işaretçi (oyunda kapalı).
                pointerPosition = testPointerPosition;
                isPressed = testPointerDown;
                pressedThisFrame = testPointerDown && !testPointerWasDown;
                testPointerWasDown = testPointerDown;
                cancel = false;
                pointerIsTouch = false;
            }
            else
            {
                pointerPosition = pointerIsTouch ? touch.position.ReadValue() : mouse.position.ReadValue();
                pressedThisFrame = pointerIsTouch ? touch.press.wasPressedThisFrame : mouse.leftButton.wasPressedThisFrame;
                isPressed = pointerIsTouch ? touchPressed : mouse.leftButton.isPressed;
                cancel = !pointerIsTouch && mouse.rightButton.wasPressedThisFrame;
            }
            if (held != null && cancel) { CancelDrag(); return; }
            if (held == null && pressedThisFrame && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            { hover = null; return; }
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
            // Boş yere tıklamak seçimi kaldırır.
            if (pressedThisFrame && hover == null && held == null) Selected = null;
            if (pressedThisFrame && hover != null)
            {
                Selected = hover;
                if (hover.IsAnchored || hover.IsPlacementGraceActive || (hover.carryable && allowCarry) ||
                    (mouseRequiresReach && !CanReach(hover))) return;
                usingTouch = pointerIsTouch;
                held = hover;
                heldManipulationNotified = false;
                heldCollider = held.GetComponent<BoxCollider>();
                previousGravity = held.Body.useGravity;
                previousKinematic = held.Body.isKinematic;
                held.Body.linearVelocity = Vector3.zero;
                held.Body.angularVelocity = Vector3.zero;
                held.Body.useGravity = false;
                // Kinematik gövde ContinuousDynamic'te MovePosition'ı yok sayar; tutarken speculative kullan.
                previousDetection = held.Body.collisionDetectionMode;
                held.Body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                held.Body.isKinematic = true;
                startWorld = held.Body.position;
                startLocal = houseRoot != null ? houseRoot.InverseTransformPoint(startWorld) : startWorld;
                // Serbest sürüklemede eşya katların içinden geçebilsin; bırakınca en yakın kata oturur.
                if (freeDrag && heldCollider != null) heldCollider.enabled = false;
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
                    if (houseRoot != null)
                    {
                        var local = houseRoot.InverseTransformPoint(target);
                        if (freeDrag)
                        {
                            // Serbest: sağa–sola ve katlar arasında; ev sınırları içinde kalır.
                            var tops = FloorTops();
                            var halfWidth = HeldHalfWidth();
                            local.x = Mathf.Clamp(local.x, -roomHalfWidth + halfWidth, roomHalfWidth - halfWidth);
                            local.y = Mathf.Clamp(local.y, tops[0], tops[tops.Length - 1] + 1.9f);
                            target = houseRoot.TransformPoint(new Vector3(local.x, local.y, startLocal.z));
                        }
                        else
                        {
                            // Yalnız yatay: evin yerel X'i değişir; yükseklik ve derinlik sabit kalır.
                            target = houseRoot.TransformPoint(new Vector3(local.x, startLocal.y, startLocal.z));
                        }
                    }
                    target.z = held.Body.position.z;
                    if (!freeDrag) ClampToFloor();
                }
                if (!heldManipulationNotified && houseRoot != null &&
                    Vector3.Distance(houseRoot.InverseTransformPoint(target), startLocal) > 0.08f)
                {
                    heldManipulationNotified = true;
                    ManipulationStarted?.Invoke();
                }
                if (!isPressed) ReleasePointer();
            }
        }
        private void LateUpdate()
        {
            // İmlecin altındaki veya tutulan eşya sıcak, seçili eşya hafif yeşil parlar.
            var target = held != null ? held : hover != null ? hover : Selected;
            if (!canDrag && held == null) target = null;
            var color = target != null && (target == held || target == hover) ? hoverTint : selectedTint;
            if (target == tintedItem && color == tintedColor) return;
            ApplyTint(tintedRenderers, null);
            tintedItem = target;
            tintedColor = color;
            tintedRenderers = target != null ? target.GetComponentsInChildren<Renderer>() : null;
            ApplyTint(tintedRenderers, color);
        }
        private void ApplyTint(Renderer[] renderers, Color? color)
        {
            if (renderers == null) return;
            if (tintBlock == null) tintBlock = new MaterialPropertyBlock();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                if (color == null) { r.SetPropertyBlock(null); continue; }
                r.GetPropertyBlock(tintBlock);
                tintBlock.SetColor("_BaseColor", color.Value);
                r.SetPropertyBlock(tintBlock);
            }
        }
        private void FixedUpdate()
        {
            SettleDropped();
            if (carried != null && resident != null && houseRoot != null)
            {
                var local = houseRoot.InverseTransformPoint(resident.transform.position);
                var halfWidth = carriedCollider != null ? carriedCollider.size.x * 0.5f : 0.3f;
                var halfHeight = carriedCollider != null ? carriedCollider.size.y * 0.5f : 0.3f;
                local.x = Mathf.Clamp(local.x + resident.Facing * (0.3f + halfWidth), -(roomHalfWidth - 0.18f), roomHalfWidth - 0.18f);
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
            if (freeDrag)
            {
                // Collider kapalıyken doğrudan konumla: katların içinden geçerken takılma olmaz.
                position += step;
                held.Body.position = position;
                held.transform.position = position;
                return;
            }
            position += AllowedMove(position, new Vector3(step.x, 0f, 0f));
            position += AllowedMove(position, new Vector3(0f, step.y, 0f));
            held.Body.MovePosition(position);
        }

        // ---------------------------------------------------------------- serbest sürükleme
        /// <summary>Katların zemin üstleri (houseRoot yerel Y), aşağıdan yukarı. Kontrolcü atar.</summary>
        public System.Func<float[]> floorTopsProvider;
        private static readonly float[] DefaultFloorTops = { -1.15f, 1.10f };

        private float[] FloorTops()
        {
            var tops = floorTopsProvider != null ? floorTopsProvider() : null;
            return tops != null && tops.Length > 0 ? tops : DefaultFloorTops;
        }

        private float HeldHalfWidth()
        {
            if (heldCollider == null) return 0.3f;
            var half = Vector3.Scale(heldCollider.size * 0.5f, Abs(heldCollider.transform.lossyScale));
            var right = houseRoot.right;
            return Mathf.Abs(Vector3.Dot(right, heldCollider.transform.right)) * half.x
                + Mathf.Abs(Vector3.Dot(right, heldCollider.transform.up)) * half.y
                + Mathf.Abs(Vector3.Dot(right, heldCollider.transform.forward)) * half.z;
        }

        /// <summary>Bırakılan eşyayı en yakın katın zeminine, boş bir yere oturtur; yer yoksa başladığı yere döner.</summary>
        private void DropOnNearestFloor()
        {
            if (held == null || houseRoot == null || heldCollider == null) return;
            var body = held.Body;
            var bodyLocal = houseRoot.InverseTransformPoint(body.position);
            var centerLocal = houseRoot.InverseTransformPoint(heldCollider.transform.TransformPoint(heldCollider.center));
            var pivotAboveCenter = bodyLocal.y - centerLocal.y;
            var half = Vector3.Scale(heldCollider.size * 0.5f, Abs(heldCollider.transform.lossyScale));
            var halfHeight = Mathf.Abs(Vector3.Dot(houseRoot.up, heldCollider.transform.right)) * half.x
                + Mathf.Abs(Vector3.Dot(houseRoot.up, heldCollider.transform.up)) * half.y
                + Mathf.Abs(Vector3.Dot(houseRoot.up, heldCollider.transform.forward)) * half.z;
            var bottom = centerLocal.y - halfHeight;
            var tops = FloorTops();
            var floorTop = tops[0];
            foreach (var top in tops) if (Mathf.Abs(top - bottom) < Mathf.Abs(floorTop - bottom)) floorTop = top;
            var halfWidth = HeldHalfWidth();
            var centerY = floorTop + halfHeight + 0.02f;
            var offsets = new[] { 0f, 0.35f, -0.35f, 0.7f, -0.7f, 1.05f, -1.05f, 1.4f, -1.4f };
            foreach (var dx in offsets)
            {
                var x = Mathf.Clamp(centerLocal.x + dx, -roomHalfWidth + halfWidth, roomHalfWidth - halfWidth);
                var worldCenter = houseRoot.TransformPoint(new Vector3(x, centerY, centerLocal.z));
                var blocked = false;
                foreach (var hit in UnityEngine.Physics.OverlapBox(worldCenter, half * 0.95f,
                    heldCollider.transform.rotation, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform.IsChildOf(held.transform)) continue;
                    if (hit.GetComponentInParent<L1_MovableProp>() != null || hit.attachedRigidbody != null) { blocked = true; break; }
                }
                if (blocked) continue;
                var pivot = houseRoot.TransformPoint(new Vector3(x + (bodyLocal.x - centerLocal.x), centerY + pivotAboveCenter, bodyLocal.z));
                body.position = pivot;
                held.transform.position = pivot;
                return;
            }
            // Yer bulunamadı: başladığı yere dön.
            var back = houseRoot.TransformPoint(startLocal);
            body.position = back;
            held.transform.position = back;
            dropBlockedUntil = Time.unscaledTime + 1.5f;
        }

        [Header("Test (Editor)")]
        [Tooltip("Açıkken fare yerine aşağıdaki sanal işaretçi kullanılır. Yalnız otomatik testler içindir.")]
        public bool testPointer;
        public Vector2 testPointerPosition;
        public bool testPointerDown;
        private bool testPointerWasDown;
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
            local.x = Mathf.Clamp(local.x, -(roomHalfWidth + 0.87f) + width, roomHalfWidth + 0.87f - width);
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
            if (heldCollider != null && !heldCollider.enabled)
            {
                DropOnNearestFloor();
                heldCollider.enabled = true;
            }
            held.Body.isKinematic = previousKinematic;
            held.Body.collisionDetectionMode = previousDetection;
            held.Body.useGravity = previousGravity;
            if (!held.Body.isKinematic)
            {
                held.Body.linearVelocity = Vector3.zero;
                held.Body.angularVelocity = Vector3.zero;
            }
            held = null;
            heldCollider = null;
            usingTouch = false;
            heldManipulationNotified = false;
        }
        private void CancelDrag()
        {
            if (held == null) return;
            var item = held;
            var back = houseRoot != null ? houseRoot.TransformPoint(startLocal) : startWorld;
            ReleasePointer();
            item.Body.position = back;
            item.transform.position = back;
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
                if (item.IsAnchored || item.IsPlacementGraceActive || !CanReach(item)) continue;
                if (carryOnlyMarked && !item.carryable) continue;
                var distance = Mathf.Abs(houseRoot.InverseTransformPoint(item.transform.position).x - player.x);
                if (distance >= best) continue;
                best = distance;
                nearest = item;
            }
            if (nearest == null) return;
            ReleasePointer();
            Selected = carried = nearest;
            ManipulationStarted?.Invoke();
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
                if (Mathf.Abs(x) + half.x > roomHalfWidth - 0.08f) continue;
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
                destination = houseRoot.TransformPoint(new Vector3(Mathf.Clamp(preferred, -(roomHalfWidth - 0.33f), roomHalfWidth - 0.33f),
                    floorY + half.y + 0.04f, 0.25f));
            carried.Body.position = destination;
            carried.transform.position = destination;
            settleBody = carried.Body;
            settlePosition = destination;
            settleSteps = 2;
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
            if (!showLegacyHint || L1_TouchControls.Available || !canDrag) return;
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(16, 16, 8, 8) };
                label.normal.textColor = L1_PaperGui.Ink;
            }
            var hintY = Selected != null ? Screen.height - 122 : Screen.height - 64;
            var hintRect = new Rect(22, hintY, 795, 42);
            L1_PaperGui.Panel(hintRect, L1_PaperGui.Parchment, L1_PaperGui.Edge);
            GUI.Label(hintRect, "Sol fare  Tut ve sağa–sola sürükle    •    Sağ fare  İptal    •    F  Sabitle    •    Tekerlek  Yakınlaş", label);
            var item = carried != null ? carried : held != null ? held : hover;
            if (item != null)
            {
                var itemRect = new Rect(22, hintY - 44, 440, 38);
                L1_PaperGui.Panel(itemRect, L1_PaperGui.Parchment, L1_PaperGui.Edge);
                GUI.Label(itemRect, item.displayName + "  •  " + item.Mass + " kg" +
                    (carried != null ? "  •  Taşınıyor" : item.IsAnchored ? "  •  Sabitli" : "  •  Sürükle"), label);
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
