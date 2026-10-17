using System.Collections.Generic;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Duvarları düz sıva yapar; rastgele seçilen bazı duvarlara farklı boyut, konum ve açıda
    /// düzensiz kenarlı çatlak/dökülmüş sıva yamaları ekler. Yamalar kaydedilmeyen geçici nesnelerdir
    /// (HideFlags.DontSave); seçim duvarın dünya konumundan türetildiği için her kat farklı görünür,
    /// her açılışta aynı kalır. Materyaller ve sahne değişmez.
    /// </summary>
    [ExecuteAlways]
    public sealed class L1_WallVariation : MonoBehaviour
    {
        [Tooltip("Duvar materyali (yalnız bunu kullanan renderer'lar etkilenir).")]
        [SerializeField] private Material wallMaterial;
        [Tooltip("Düz sıvanın rengi.")]
        [SerializeField] private Color plasterTint = new Color(0.93f, 0.88f, 0.8f, 1f);
        [Header("Hasar yamaları")]
        [Tooltip("Düzensiz kenarlı çatlak yaması materyalleri (alfa kesmeli).")]
        [SerializeField] private Material[] patchMaterials;
        [Tooltip("Bir duvar parçasının hasarlı olma olasılığı.")]
        [SerializeField, Range(0f, 1f)] private float damagedWallChance = 0.5f;
        [Tooltip("Hasarlı duvardaki yama sayısı (en az–en çok).")]
        [SerializeField] private Vector2Int patchesPerWall = new Vector2Int(1, 3);
        [Tooltip("Yama boyutu (metre, en küçük–en büyük).")]
        [SerializeField] private Vector2 patchSize = new Vector2(0.3f, 1.25f);
        [Tooltip("Pencereli/kapılı duvarlara yama konmasın (açıklığın üstüne binmesin).")]
        [SerializeField] private bool skipOpenings = true;
        [SerializeField] private int seed = 17;

        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int BumpMapSt = Shader.PropertyToID("_BumpMap_ST");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

        private readonly List<GameObject> patches = new List<GameObject>();
        private Mesh quad;

        private void OnEnable() => Apply();
        private void Start() => Apply();
        private void OnDisable() => ClearPatches();
#if UNITY_EDITOR
        private void OnValidate()
        {
            // OnValidate'te nesne oluşturmak yasak; bir sonraki editör karesinde uygula.
            UnityEditor.EditorApplication.delayCall += () => { if (this != null && isActiveAndEnabled) Apply(); };
        }
#endif

        public void Apply()
        {
            if (wallMaterial == null) return;
            ClearPatches();
            var block = new MaterialPropertyBlock();
            var roomCenter = transform.position;
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterial != wallMaterial) continue;
                // Tüm duvarlar düz sıva.
                r.GetPropertyBlock(block);
                block.Clear();
                block.SetTexture(BaseMap, Texture2D.whiteTexture);
                block.SetColor(BaseColor, plasterTint);
                block.SetVector(BumpMapSt, new Vector4(0.12f, 0.12f, 0.3f, 0.6f));
                block.SetFloat(BumpScale, 0.35f);
                r.SetPropertyBlock(block);

                if (patchMaterials == null || patchMaterials.Length == 0) continue;
                var filter = r.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var meshName = filter.sharedMesh.name;
                if (skipOpenings && (meshName.Contains("Window") || meshName.Contains("Door"))) continue;

                var p = r.transform.position;
                var random = new System.Random(Hash(Mathf.RoundToInt(p.x * 10f), Mathf.RoundToInt(p.y * 10f),
                    Mathf.RoundToInt(p.z * 10f) + seed));
                if (random.NextDouble() > damagedWallChance) continue;
                AddPatches(r, filter.sharedMesh.bounds, roomCenter, random);
            }
        }

        private void AddPatches(Renderer wall, Bounds local, Vector3 roomCenter, System.Random random)
        {
            var t = wall.transform;
            // Duvarın ince ekseni yerel Z; odaya bakan yüzü seç.
            var toRoom = roomCenter - t.TransformPoint(local.center);
            var side = Vector3.Dot(t.forward, toRoom) >= 0f ? 1f : -1f;
            var normal = t.forward * side;
            var count = random.Next(patchesPerWall.x, patchesPerWall.y + 1);
            for (var i = 0; i < count; i++)
            {
                var size = Mathf.Lerp(patchSize.x, patchSize.y, (float)(random.NextDouble() * random.NextDouble() * 1.4f));
                var u = Mathf.Lerp(0.12f, 0.88f, (float)random.NextDouble());
                var v = Mathf.Lerp(0.15f, 0.85f, (float)random.NextDouble());
                var localPoint = new Vector3(
                    Mathf.Lerp(local.min.x, local.max.x, u),
                    Mathf.Lerp(local.min.y, local.max.y, v),
                    local.center.z + side * (local.extents.z + 0.004f + i * 0.002f));
                var world = t.TransformPoint(localPoint);
                var go = new GameObject("Çatlak yaması") { hideFlags = HideFlags.DontSave | HideFlags.NotEditable };
                go.transform.SetParent(transform, true);
                go.transform.position = world;
                go.transform.rotation = Quaternion.LookRotation(-normal, transform.up) *
                    Quaternion.Euler(0f, 0f, (float)random.NextDouble() * 360f);
                var parentScale = transform.lossyScale;
                go.transform.localScale = new Vector3(size / parentScale.x, size / parentScale.y, 1f);
                go.AddComponent<MeshFilter>().sharedMesh = Quad();
                var renderer = go.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = patchMaterials[random.Next(patchMaterials.Length)];
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                patches.Add(go);
            }
        }

        private Mesh Quad()
        {
            if (quad != null) return quad;
            quad = new Mesh { name = "L1 çatlak yaması", hideFlags = HideFlags.DontSave };
            quad.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) };
            quad.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
            quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            quad.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            quad.tangents = new[] { new Vector4(1f, 0f, 0f, -1f), new Vector4(1f, 0f, 0f, -1f),
                new Vector4(1f, 0f, 0f, -1f), new Vector4(1f, 0f, 0f, -1f) };
            return quad;
        }

        private void ClearPatches()
        {
            foreach (var p in patches)
            {
                if (p == null) continue;
                if (Application.isPlaying) Destroy(p); else DestroyImmediate(p);
            }
            patches.Clear();
            // Derleme/yeniden yüklemeden kalan yetim yamaları da temizle.
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
                if (child.name != "Çatlak yaması") continue;
                if (Application.isPlaying) Destroy(child); else DestroyImmediate(child);
            }
        }

        private static int Hash(int x, int y, int z)
        {
            unchecked
            {
                var h = 17;
                h = h * 31 + x;
                h = h * 31 + y;
                h = h * 31 + z;
                return h;
            }
        }
    }
}
