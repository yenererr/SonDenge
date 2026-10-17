using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Duvar parçalarına çeşitlilik verir: bazıları düz sıva, bazıları farklı ölçek ve konumda çatlaklı.
    /// MaterialPropertyBlock kullanır; materyal ve sahne değişmez. Seçim, parçanın dünya konumundan
    /// türetildiği için her kat farklı görünür ama her açılışta aynı kalır.
    /// </summary>
    [ExecuteAlways]
    public sealed class L1_WallVariation : MonoBehaviour
    {
        [Tooltip("Çeşitlendirilecek duvar materyali (yalnız bunu kullanan renderer'lar etkilenir).")]
        [SerializeField] private Material wallMaterial;
        [Tooltip("Düz (çatlaksız) sıva olma olasılığı.")]
        [SerializeField, Range(0f, 1f)] private float cleanChance = 0.45f;
        [Tooltip("Çatlaklı parçalarda doku ölçeği aralığı (küçük = büyük çatlak).")]
        [SerializeField] private Vector2 crackTilingRange = new Vector2(0.16f, 0.5f);
        [Tooltip("Düz sıvanın rengi (dokunun ortalama tonuna yakın).")]
        [SerializeField] private Color cleanTint = new Color(0.93f, 0.88f, 0.8f, 1f);
        [SerializeField] private int seed = 17;

        private static readonly int BaseMapSt = Shader.PropertyToID("_BaseMap_ST");
        private static readonly int BumpMapSt = Shader.PropertyToID("_BumpMap_ST");
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int BumpScale = Shader.PropertyToID("_BumpScale");

        private void OnEnable() => Apply();
        private void Start() => Apply();
#if UNITY_EDITOR
        private void OnValidate() => Apply();
#endif

        public void Apply()
        {
            if (wallMaterial == null) return;
            var block = new MaterialPropertyBlock();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
            {
                if (r.sharedMaterial != wallMaterial) continue;
                var p = r.transform.position;
                var hash = Hash(Mathf.RoundToInt(p.x * 10f), Mathf.RoundToInt(p.y * 10f), Mathf.RoundToInt(p.z * 10f) + seed);
                var random = new System.Random(hash);
                r.GetPropertyBlock(block);
                block.Clear();
                if (random.NextDouble() < cleanChance)
                {
                    // Düz sıva: doku yerine beyaz + sıcak krem ton; hafif yüzey dokusu kalsın.
                    block.SetTexture(BaseMap, Texture2D.whiteTexture);
                    block.SetColor(BaseColor, cleanTint);
                    block.SetVector(BumpMapSt, new Vector4(0.12f, 0.12f, (float)random.NextDouble(), (float)random.NextDouble()));
                    block.SetFloat(BumpScale, 0.35f);
                }
                else
                {
                    var tiling = Mathf.Lerp(crackTilingRange.x, crackTilingRange.y, (float)random.NextDouble());
                    var st = new Vector4(tiling, tiling, (float)random.NextDouble(), (float)random.NextDouble());
                    block.SetVector(BaseMapSt, st);
                    block.SetVector(BumpMapSt, st);
                }
                r.SetPropertyBlock(block);
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
