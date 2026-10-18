using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Şiddetli yağmurda evin içini yavaş yavaş dolduran su: ev genişliğinde yarı saydam bir su kütlesi ve
    /// hafif dalgalanan açık renkli yüzey. Yükseklik L1_ProjectGameController tarafından (ev yerel uzayında) verilir.
    /// </summary>
    public sealed class L1_FloodWater : MonoBehaviour
    {
        [SerializeField] private Color bodyColor = new Color(0.16f, 0.36f, 0.52f, 0.62f);
        [SerializeField] private Color surfaceColor = new Color(0.52f, 0.74f, 0.86f, 0.75f);
        [SerializeField] private float waveAmplitude = 0.025f;

        private Transform body, surface;
        private Material bodyMaterial, surfaceMaterial;
        private float bottom, top, halfWidth, depth, frontZ;

        /// <summary>Evin içine (houseRoot altına) su kütlesini kurar. bottomY: suyun tabanı (ev yerel y).</summary>
        public static L1_FloodWater Create(Transform houseRoot, float bottomY, float houseHalfWidth, float frontZ, float depth)
        {
            var go = new GameObject("Sel suyu");
            go.transform.SetParent(houseRoot, false);
            var flood = go.AddComponent<L1_FloodWater>();
            flood.bottom = bottomY;
            flood.halfWidth = houseHalfWidth;
            flood.frontZ = frontZ;
            flood.depth = depth;
            flood.Build();
            return flood;
        }

        private void Build()
        {
            bodyMaterial = MakeMaterial(bodyColor);
            surfaceMaterial = MakeMaterial(surfaceColor);
            body = Box("Su kütlesi", bodyMaterial);
            surface = Box("Su yüzeyi", surfaceMaterial);
            SetTop(bottom);
        }

        private Transform Box(string name, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(cube.GetComponent<Collider>());
            cube.name = name;
            cube.transform.SetParent(transform, false);
            var r = cube.GetComponent<Renderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return cube.transform;
        }

        private static Material MakeMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
            var m = new Material(shader);
            // URP Unlit'i saydam yap.
            if (m.HasProperty("_Surface"))
            {
                m.SetFloat("_Surface", 1f);
                m.SetFloat("_Blend", 0f);
                m.SetOverrideTag("RenderType", "Transparent");
                m.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0f);
                m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            m.color = color;
            return m;
        }

        /// <summary>Su yüzeyinin ev yerel uzayındaki yüksekliği.</summary>
        public void SetTop(float topY)
        {
            top = Mathf.Max(bottom, topY);
            var h = Mathf.Max(0.001f, top - bottom);
            body.localPosition = new Vector3(0f, bottom + h * 0.5f, frontZ + depth * 0.5f);
            body.localScale = new Vector3(halfWidth * 2f, h, depth);
            surface.localScale = new Vector3(halfWidth * 2f, 0.04f, depth);
            body.gameObject.SetActive(h > 0.01f);
            surface.gameObject.SetActive(h > 0.01f);
        }

        public void SetVisible(bool on) => gameObject.SetActive(on);

        private void Update()
        {
            if (surface == null || !surface.gameObject.activeSelf) return;
            var wave = GameSettings() ? 0f : Mathf.Sin(Time.time * 2.3f) * waveAmplitude;
            surface.localPosition = new Vector3(0f, top + wave, frontZ + depth * 0.5f);
        }

        private static bool GameSettings() => Jam.UI.GameSettings.ReduceMotion;

        private void OnDestroy()
        {
            if (bodyMaterial != null) Destroy(bodyMaterial);
            if (surfaceMaterial != null) Destroy(surfaceMaterial);
        }
    }
}
