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
        }
    }
}
