using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Ev içinde görev hedef alanı (yerel kutu). Evin pivotu döndüğü için ölçüm bu nesnenin
    /// yerel uzayında yapılır; eğimde sağ-sol işareti bozulmaz. Üstünde yumuşak bir işaret halkası gösterir.
    /// </summary>
    public sealed class L1_MissionZone : MonoBehaviour
    {
        public string zoneId;
        [Tooltip("Alanın yerel boyutu (m).")]
        public Vector3 size = new Vector3(1.2f, 1.4f, 2f);
        public Color markerColor = new Color(1f, 0.82f, 0.25f, 0.85f);
        public bool showMarker = true;

        private Transform marker;
        private Material markerMaterial;
        private float glowUntil;

        public bool Contains(Vector3 worldPoint)
        {
            var local = transform.InverseTransformPoint(worldPoint);
            var half = size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        public void SetMarkerVisible(bool visible)
        {
            showMarker = visible;
            if (marker != null) marker.gameObject.SetActive(visible);
        }

        /// <summary>İpucu: işareti birkaç saniye belirgin parlatır.</summary>
        public void Glow(float seconds) => glowUntil = Time.unscaledTime + seconds;

        private void Start() => BuildMarker();

        private void BuildMarker()
        {
            if (marker != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            markerMaterial = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
            SetMaterialColor(markerColor);
            marker = new GameObject("Görev işareti").transform;
            marker.SetParent(transform, false);
            marker.localPosition = new Vector3(0f, -size.y * 0.5f + 0.03f, -0.2f);
            // Zemin üstünde ince halka (dört kenar) + üstte süzülen ünlem çubuğu.
            var w = size.x;
            Bar(new Vector3(0f, 0f, 0f), new Vector3(w, 0.05f, 0.05f));
            Bar(new Vector3(-w * 0.5f, 0.12f, 0f), new Vector3(0.05f, 0.24f, 0.05f));
            Bar(new Vector3(w * 0.5f, 0.12f, 0f), new Vector3(0.05f, 0.24f, 0.05f));
            Bar(new Vector3(0f, size.y + 0.15f, 0f), new Vector3(0.09f, 0.3f, 0.09f), true);
            Bar(new Vector3(0f, size.y - 0.07f, 0f), new Vector3(0.09f, 0.09f, 0.09f), true);
            marker.gameObject.SetActive(showMarker);
        }

        private void Bar(Vector3 position, Vector3 scale, bool floating = false)
        {
            var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(bar.GetComponent<Collider>());
            bar.name = floating ? "Ünlem" : "Kenar";
            bar.transform.SetParent(marker, false);
            bar.transform.localPosition = position;
            bar.transform.localScale = scale;
            var renderer = bar.GetComponent<Renderer>();
            renderer.sharedMaterial = markerMaterial;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private void SetMaterialColor(Color c)
        {
            if (markerMaterial == null) return;
            if (markerMaterial.HasProperty("_BaseColor")) markerMaterial.SetColor("_BaseColor", c);
            markerMaterial.color = c;
        }

        private void Update()
        {
            if (marker == null || !marker.gameObject.activeSelf) return;
            var glowing = Time.unscaledTime < glowUntil;
            var pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * (glowing ? 9f : 3f));
            var c = markerColor;
            c.a = Mathf.Lerp(glowing ? 0.65f : 0.35f, 1f, pulse);
            if (glowing) c = Color.Lerp(c, Color.white, 0.35f * pulse);
            SetMaterialColor(c);
            marker.localScale = Vector3.one * (glowing ? 1f + 0.08f * pulse : 1f);
        }

        private void OnDestroy()
        {
            if (markerMaterial != null) Destroy(markerMaterial);
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
            Gizmos.DrawCube(Vector3.zero, size);
        }
    }
}
