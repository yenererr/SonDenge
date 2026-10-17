using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Var olan oynanabilir 3D evi ve arka planı yerinde düzenler; eşyaları ve oyun bileşenlerini korur.</summary>
    internal static class L1_AsymmetricHouseLayout
    {
        private static readonly float[] FloorX = { -0.50f, 0.55f, -0.70f };
        private static readonly float[] FloorY = { -1.15f, 1.10f, 3.35f };
        private const string BeamsName = "[L1] Dışa uzanan düzensiz tahtalar";

        [MenuItem("Son Denge/Level1/Katları Asimetrik Yerleştir")]
        private static void ApplyScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Katları düzenlemek için Play modundan çıkın.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Level1 sahnesini açın.");
            var world = GameObject.Find("[L1] World Visuals");
            if (world == null) throw new InvalidOperationException("3D dünya bulunamadı.");
            var house = world.GetComponentInChildren<L1_ProjectGameController>(true)?.houseRoot;
            if (house == null) throw new InvalidOperationException("Oynanabilir 3D ev bulunamadı.");
            ApplyToHouse(house);
            LowerMountains(world.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Katlar asimetrik yerleştirildi; merdiven ve eşyalar hizalandı, dağlar alçaltıldı.");
        }

        internal static void ApplyToHouse(Transform house)
        {
            var delta = new float[3];
            for (var level = 0; level < 3; level++)
            {
                var floor = house.Find("Kat " + (level + 1));
                if (floor == null) continue;
                delta[level] = FloorX[level] - floor.localPosition.x;
                var position = floor.localPosition;
                position.x = FloorX[level];
                floor.localPosition = position;
                AddUnevenBoards(floor, level);
            }

            // Hareketli eşyalar kat kökünün altında değildir; döşemeyle aynı miktarda taşınırlar.
            var furniture = house.Find("Bağımsız 3D mobilyalar");
            if (furniture != null)
            {
                foreach (Transform item in furniture)
                {
                    var index = NearestFloor(item.localPosition.y);
                    item.localPosition += Vector3.right * delta[index];
                    // Sonradan açılan alt kat merdiveninin üst çıkışını açık tut.
                    if (item.name.Contains("ocak ve fırın")) MoveX(item, -0.20f);
                }
            }

            MoveX(house.Find("Zemin Kazıkları ve İskele Tabanı"), FloorX[0]);
            MoveX(house.Find("Düz çatı"), FloorX[1]);
            MoveX(house.Find("Ahşap balkon ve vinç"), FloorX[1]);
            MoveX(house.Find("[L1] Üçüncü kat balkon ve vinç parçaları"), FloorX[2]);
            MoveX(house.Find("Asimetrik kiremit çatı"), FloorX[1]);
            PlaceLadder(house.Find("Alt kattan orta kata"), house, 1.75f);
            PlaceLadder(house.Find("Orta kattan çatı katına"), house, -0.85f);
        }

        private static int NearestFloor(float y)
        {
            var index = 0;
            var distance = Mathf.Abs(y - FloorY[0]);
            for (var i = 1; i < FloorY.Length; i++)
            {
                var next = Mathf.Abs(y - FloorY[i]);
                if (next >= distance) continue;
                index = i;
                distance = next;
            }
            return index;
        }

        private static void MoveX(Transform item, float x)
        {
            if (item == null) return;
            var position = item.localPosition;
            position.x = x;
            item.localPosition = position;
        }

        private static void PlaceLadder(Transform item, Transform house, float x)
        {
            if (item == null) return;
            var route = item.GetComponent<L1_ClimbRoute>();
            if (route == null || route.bottom == null) return;
            var position = item.localPosition;
            position.x += x - house.InverseTransformPoint(route.bottom.position).x;
            item.localPosition = position;
        }

        private static void AddUnevenBoards(Transform floor, int level)
        {
            var old = floor.Find(BeamsName);
            if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Levels/Level1/Meshes/L1_HandHewnBeam.asset");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Levels/Level1/Materials/L1_3D_WornOak.mat");
            if (mesh == null || material == null) return;
            var root = new GameObject(BeamsName).transform;
            root.SetParent(floor, false);
            var y = FloorY[level];
            var side = level == 1 ? 1f : -1f;
            for (var i = 0; i < 4; i++)
            {
                var z = -1.22f + i * 0.70f;
                var reach = 0.56f + (i % 3) * 0.17f;
                Beam(root, mesh, material, "Dışa taşan aşınmış döşeme " + i,
                    new Vector3(side * 3.05f, y - 0.10f, z),
                    new Vector3(side * (3.66f + reach), y - 0.14f - 0.02f * i, z), 0.15f, 0.24f);
            }
            Beam(root, mesh, material, "Dış platform kenarı",
                new Vector3(side * 4.12f, y - 0.21f, -1.39f),
                new Vector3(side * 4.20f, y - 0.18f, 1.07f), 0.18f, 0.19f);
            Beam(root, mesh, material, "Eski çapraz payanda",
                new Vector3(side * 3.08f, y - 0.74f, -1.00f),
                new Vector3(side * 4.05f, y - 0.18f, -1.00f), 0.17f, 0.19f);
        }

        private static void Beam(Transform parent, Mesh mesh, Material material, string name,
            Vector3 from, Vector3 to, float width, float depth)
        {
            var part = new GameObject(name).transform;
            part.SetParent(parent, false);
            part.localPosition = (from + to) * 0.5f;
            part.localRotation = Quaternion.FromToRotation(Vector3.right, (to - from).normalized);
            part.localScale = new Vector3(Vector3.Distance(from, to), width, depth);
            part.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            part.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void LowerMountains(Transform world)
        {
            var peaks = world.Find("[L1] Bulut denizi ve dağ uçları/Dağ zirveleri");
            if (peaks == null) return;
            foreach (Transform mountain in peaks)
            {
                var renderers = mountain.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                var tip = mountain.name.Contains("uzak") ? -5.7f : -4.2f;
                mountain.position += Vector3.up * (tip - bounds.max.y);
            }
        }
    }
}
