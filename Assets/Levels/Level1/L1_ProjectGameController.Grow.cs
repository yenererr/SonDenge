using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Bölüm ilerledikçe evin aşağı doğru büyümesi: en alttaki katın altına boş bir kat eklenir,
    /// ev kayanın üstünde bir kat yüksekliği kadar kalkar. Mevcut katlar, eşyalar ve fizik korunur.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        [Header("Aşağı doğru büyüme")]
        [Tooltip("Açıksa 'Sonraki bölüm' aynı sahnede bir sonraki afete geçer ve en alta yeni kat eklenir.")]
        public bool growDownward = true;
        [Tooltip("Eklenecek boş kat görseli (L1_EmptyFloor_Medieval).")]
        public GameObject newFloorPrefab;
        [Tooltip("Kopyalanacak mevcut en alt kat (zemin ve yan duvar collider'ları, lambalar için şablon).")]
        public Transform floorTemplate;
        [Tooltip("Katlar arası kiriş döşemesi şablonu (çatı döşemesi kopyası; döşeme tahtası çıkarılır).")]
        public Transform slabTemplate;
        [Tooltip("Evin altındaki taban kirişleri; her yeni katta en alta iner.")]
        public Transform baseBeams;
        [Tooltip("Kopyalanacak merdiven rotası (alt kattan bir üst kata).")]
        public L1_ClimbRoute ladderTemplate;
        public float floorHeight = 2.25f;
        public float growSeconds = 1.6f;
        [Tooltip("En alt katın zemin üstü (houseRoot yerel Y). Büyüdükçe azalır.")]
        public float lowestFloorTop = -1.15f;

        private Collider lowestFloorCollider;
        private int addedFloors;
        private bool growing;

        public bool IsGrowing => growing;
        public int FloorCount => 2 + addedFloors;

        private void GrowFloorBelow()
        {
            if (growing || houseRoot == null || newFloorPrefab == null || floorTemplate == null) return;
            StartCoroutine(GrowRoutine());
        }

        private IEnumerator GrowRoutine()
        {
            growing = true;
            if (drag != null) drag.canDrag = false;
            addedFloors++;
            var newTop = lowestFloorTop - floorHeight;
            var group = new GameObject("Alt kat " + addedFloors + " (yeni)").transform;
            group.SetParent(houseRoot, false);

            // Görsel kat.
            var visual = Instantiate(newFloorPrefab, group);
            visual.transform.localPosition = new Vector3(0f, newTop, 0f);

            // Oyun collider'ları ve lambalar: mevcut en alt kattan kopya, bir kat aşağıda.
            Collider newFloorCollider = null;
            foreach (Transform child in floorTemplate)
            {
                var isFloor = child.name == "Yürünebilir zemin";
                var isWall = child.name == "Yan duvar çarpışması";
                var isLamp = child.name == "Modular Sewer — Sarkan Lamba";
                if (!child.gameObject.activeSelf || (!isFloor && !isWall && !isLamp)) continue;
                var copy = Instantiate(child.gameObject, group);
                copy.name = child.name;
                copy.transform.localPosition = child.localPosition + Vector3.up * (newTop - templateFloorTop);
                copy.transform.localRotation = child.localRotation;
                if (isFloor) newFloorCollider = copy.GetComponent<Collider>();
            }

            // Yeni kat ile üstündeki kat arasındaki kirişler.
            if (slabTemplate != null)
            {
                var slab = Instantiate(slabTemplate.gameObject, group);
                slab.name = "Ara kat kirişleri";
                var remove = new List<GameObject>();
                foreach (Transform c in slab.transform)
                    if (c.name.Contains("döşeme tahtası") || c.name.Contains("ahşap döşeme")) remove.Add(c.gameObject);
                foreach (var r in remove) Destroy(r);
                foreach (var col in slab.GetComponentsInChildren<Collider>(true)) Destroy(col);
                // Şablon çatı döşemesi: en üst katın tavanı. Yeni katın tavanı = eski en alt katın zemini.
                slab.transform.localPosition = slabTemplate.localPosition + Vector3.up * (lowestFloorTop - SlabTop());
            }

            // Merdiven: yeni kattan bir üst kata.
            if (ladderTemplate != null)
            {
                var ladder = Instantiate(ladderTemplate.gameObject, group).GetComponent<L1_ClimbRoute>();
                ladder.gameObject.SetActive(true);
                ladder.transform.localPosition = ladderTemplate.transform.localPosition + Vector3.up * (newTop - templateFloorTop);
                ladder.upperFloor = lowestFloorCollider != null ? lowestFloorCollider : FindFloorCollider(floorTemplate);
                if (resident != null)
                {
                    var list = new List<L1_ClimbRoute>(resident.ladders ?? new L1_ClimbRoute[0]) { ladder };
                    resident.ladders = list.ToArray();
                }
            }
            lowestFloorCollider = newFloorCollider;
            lowestFloorTop = newTop;
            if (baseBeams != null) baseBeams.localPosition += Vector3.down * floorHeight;

            // Evi kayanın üstünde bir kat yüksekliği kadar kaldır (eşyalar bu sırada kilitli).
            var bodies = new List<Rigidbody>();
            var kinematic = new List<bool>();
            var localPositions = new List<Vector3>();
            var interpolation = new List<RigidbodyInterpolation>();
            foreach (var body in houseRoot.GetComponentsInChildren<Rigidbody>())
            {
                bodies.Add(body);
                kinematic.Add(body.isKinematic);
                interpolation.Add(body.interpolation);
                localPositions.Add(houseRoot.InverseTransformPoint(body.position));
                if (!body.isKinematic) { body.linearVelocity = Vector3.zero; body.angularVelocity = Vector3.zero; }
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.None;
            }
            var start = houseRoot.localPosition;
            var end = start + Vector3.up * floorHeight;
            if (cameraRig == null && sceneCamera != null) cameraRig = sceneCamera.GetComponent<L1_CameraController>();
            if (cameraRig != null) cameraRig.ShiftRest(floorHeight * 0.75f, 8f);
            for (var t = 0f; t < growSeconds; t += Time.unscaledDeltaTime)
            {
                var k = Mathf.SmoothStep(0f, 1f, t / growSeconds);
                houseRoot.localPosition = Vector3.Lerp(start, end, k);
                CarryBodies(bodies, localPositions);
                yield return null;
            }
            houseRoot.localPosition = end;
            CarryBodies(bodies, localPositions);
            UnityEngine.Physics.SyncTransforms();
            for (var i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] == null) continue;
                bodies[i].isKinematic = kinematic[i];
                bodies[i].interpolation = interpolation[i];
            }
            if (resident != null) resident.spawnPosition += houseRoot.up * floorHeight;

            RefreshFurniture();
            CacheHangingLamps();
            Warn("Ev büyüdü: en alta yeni boş bir kat eklendi.");
            growing = false;
            marketOpen = true;
        }

        // Şablon kat (Kat 1) zemin üstü; kopyalar buna göre aşağı kaydırılır.
        private const float templateFloorTop = -1.15f;

        private float SlabTop()
        {
            // Çatı döşemesinin üstü, en üst katın (Kat 2) zemininden bir kat yukarıda: 1.10 + 2.25.
            return FloorY[1] + floorHeight;
        }

        // Rigidbody'ler ebeveyn transformu yükselince kendiliğinden gelmez; evle birlikte taşı.
        private void CarryBodies(List<Rigidbody> bodies, List<Vector3> localPositions)
        {
            for (var i = 0; i < bodies.Count; i++)
            {
                if (bodies[i] == null) continue;
                var world = houseRoot.TransformPoint(localPositions[i]);
                bodies[i].position = world;
                bodies[i].transform.position = world;
            }
        }

        private static Collider FindFloorCollider(Transform floor)
        {
            if (floor == null) return null;
            var t = floor.Find("Yürünebilir zemin");
            return t != null ? t.GetComponent<Collider>() : null;
        }
    }
}
