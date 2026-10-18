using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Sırttaki deprem çantasını yan görünümde görünür tutar: çanta karakterin baktığı yönün
    /// tersinde, sırt hizasında durur ve karakter dönünce öbür tarafa geçer.
    /// </summary>
    public sealed class L1_BackpackFollow : MonoBehaviour
    {
        public L1_ResidentController resident;
        [Tooltip("Karakter pivotuna göre konum: x = sırt mesafesi (yön ile çarpılır), y = yükseklik, z = derinlik.")]
        public Vector3 offset = new Vector3(0.2f, -0.14f, 0.05f);
        public float smooth = 18f;

        private float side = -1f;

        private void LateUpdate()
        {
            if (resident == null) return;
            var target = resident.Facing >= 0 ? -1f : 1f;
            side = Mathf.MoveTowards(side, target, Time.deltaTime * smooth * 0.25f);
            transform.localPosition = new Vector3(offset.x * side, offset.y + (resident.IsHiding ? -0.12f : 0f), offset.z);
            // Askılar karaktere dönük olsun.
            transform.localRotation = Quaternion.Euler(0f, side > 0f ? -90f : 90f, side * -8f);
        }
    }
}
