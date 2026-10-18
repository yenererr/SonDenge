using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Sakin karakterinin 3D modelini (KayKit Barbar) hareketle eşler: Animator'a hız ve tırmanma
    /// bilgisini verir, modeli yürüdüğü yöne çevirir (yan görünümde 3/4 açı), dururken kameraya bakar.
    /// </summary>
    public sealed class L1_CharacterAnimator : MonoBehaviour
    {
        [SerializeField] private L1_ResidentController resident;
        [SerializeField] private Animator animator;
        [Tooltip("Modelin döndürülecek kökü (genelde Animator'ın olduğu nesne).")]
        [SerializeField] private Transform model;
        [Tooltip("Dururken model açısı (180 = kameraya bakar).")]
        [SerializeField] private float idleYaw = 180f;
        [Tooltip("Yürürken kameradan ne kadar yana dönsün (derece).")]
        [SerializeField] private float walkTurn = 65f;
        [SerializeField] private float turnSpeed = 10f;
        [Tooltip("Bu hızın (m/sn) üstü 'yürüyor' sayılır.")]
        [SerializeField] private float walkThreshold = 0.15f;

        [Header("Kucakta taşıma")]
        [Tooltip("Eşya göğsün ne kadar önünde (kameraya doğru) durur (m).")]
        [SerializeField] private float carryForward = 0.3f;
        [Tooltip("Eşyanın göğüs kemiğine göre yüksekliği (m).")]
        [SerializeField] private float carryHeight = 0.2f;
        [Tooltip("Kolların eşyaya ne kadar uzandığı (0 = animasyon, 1 = tam).")]
        [SerializeField, Range(0f, 1f)] private float armReach = 0.9f;

        private Transform chest, upperL, upperR, handL, handR;
        private L1_PropDrag drag;

        private static readonly int SpeedId = Animator.StringToHash("Speed");
        private static readonly int ClimbingId = Animator.StringToHash("Climbing");
        private Rigidbody body;
        private Vector3 lastLocal;
        private float yaw;
        private float speedSmoothed;

        private void Awake()
        {
            if (resident == null) resident = GetComponentInParent<L1_ResidentController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (model == null && animator != null) model = animator.transform;
            body = resident != null ? resident.GetComponent<Rigidbody>() : null;
            yaw = idleYaw;
            if (model != null)
                foreach (var t in model.GetComponentsInChildren<Transform>(true))
                    switch (t.name)
                    {
                        case "chest": chest = t; break;
                        case "upperarm.l": upperL = t; break;
                        case "upperarm.r": upperR = t; break;
                        case "hand.l": handL = t; break;
                        case "hand.r": handR = t; break;
                    }
            drag = resident != null && resident.transform.parent != null
                ? resident.transform.parent.GetComponentInParent<L1_PropDrag>() : null;
            if (drag == null) drag = FindAnyObjectByType<L1_PropDrag>();
            if (drag != null) drag.residentHoldsCarried = chest != null;
            if (resident != null && resident.transform.parent != null)
                lastLocal = resident.transform.parent.InverseTransformPoint(resident.transform.position);
        }

        private void LateUpdate()
        {
            if (resident == null || animator == null) return;
            var frame = resident.transform.parent;
            var local = frame != null ? frame.InverseTransformPoint(resident.transform.position) : resident.transform.position;
            var dt = Mathf.Max(Time.deltaTime, 0.0001f);
            var horizontalSpeed = (local.x - lastLocal.x) / dt;
            lastLocal = local;
            speedSmoothed = Mathf.Lerp(speedSmoothed, Mathf.Abs(horizontalSpeed), 1f - Mathf.Exp(-12f * dt));

            var climbing = resident.IsClimbing;
            animator.SetBool(ClimbingId, climbing);
            animator.SetFloat(SpeedId, resident.IsHiding ? 0f : speedSmoothed);

            // Yön: tırmanırken merdivene (arkaya) bakar, yürürken yürüdüğü yöne 3/4 döner.
            float target;
            if (climbing) target = 0f;
            else if (speedSmoothed > walkThreshold) target = idleYaw - Mathf.Sign(horizontalSpeed == 0f ? resident.Facing : horizontalSpeed) * walkTurn;
            else target = idleYaw;
            yaw = Mathf.LerpAngle(yaw, target, 1f - Mathf.Exp(-turnSpeed * dt));
            if (model != null) model.localRotation = Quaternion.Euler(0f, yaw, 0f);
            HoldCarried();
        }

        /// <summary>Taşınan eşyayı göğsün önüne koyar ve iki kolu ona uzatır (ilk yardım çantası gibi kucakta).</summary>
        private void HoldCarried()
        {
            var item = resident.CarriedItem;
            if (item == null || chest == null || item.Body == null) return;
            var frame = resident.transform.parent;
            var toCamera = frame != null ? -frame.forward : Vector3.back;
            var side = frame != null ? frame.right : Vector3.right;
            var up = frame != null ? frame.up : Vector3.up;
            var center = chest.position + toCamera * carryForward + up * carryHeight + side * (resident.Facing * 0.06f);
            item.transform.position = center;
            item.Body.position = center;
            var box = item.GetComponent<BoxCollider>();
            var half = box != null ? Mathf.Clamp(box.size.x * item.transform.lossyScale.x * 0.5f, 0.08f, 0.3f) : 0.15f;
            Reach(upperL, handL, center - side * half);
            Reach(upperR, handR, center + side * half);
        }

        private void Reach(Transform upper, Transform hand, Vector3 target)
        {
            if (upper == null || hand == null) return;
            var current = hand.position - upper.position;
            var wanted = target - upper.position;
            if (current.sqrMagnitude < 1e-6f || wanted.sqrMagnitude < 1e-6f) return;
            var turn = Quaternion.FromToRotation(current, wanted);
            upper.rotation = Quaternion.Slerp(Quaternion.identity, turn, armReach) * upper.rotation;
        }
    }
}
