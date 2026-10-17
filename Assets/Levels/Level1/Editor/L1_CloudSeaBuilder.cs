using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jam.Levels.Level1
{
    /// <summary>Builds the distant, collider-free cloud sea without touching the playable house.</summary>
    internal static class L1_CloudSeaBuilder
    {
        private const string Art = "Assets/Levels/Level1/Art/CloudSea";
        private const string Clouds = Art + "/Clouds";
        private const string Mountain = Art + "/Mountain/Mountain Lowpoly.fbx";
        private const string GroupName = "[L1] Bulut denizi ve dağ uçları";
        private static readonly int[] BankClouds = { 0, 1, 2, 3, 8, 13, 14, 16 };

        [MenuItem("Son Denge/Level1/Bulut Denizi Arka Planını Kur")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Arka planı kurmadan önce Play modunu durdurun.");
                return;
            }
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Önce Level1 sahnesini açın.");
            var world = GameObject.Find("[L1] World Visuals");
            if (world == null) throw new InvalidOperationException("Level1 dünya kökü bulunamadı.");
            BuildInto(world.transform);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Bulut denizi hazır: 3D bulutlar, uzak/yakın dağ uçları ve alacakaranlık gökyüzü.");
        }

        internal static void BuildInto(Transform world)
        {
            var mountainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Mountain);
            if (mountainPrefab == null) throw new InvalidOperationException("Dağ modeli eksik: " + Mountain);
            var cloudPrefabs = new GameObject[17];
            for (var i = 0; i < cloudPrefabs.Length; i++)
            {
                cloudPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(Clouds + "/Cloud_" + i.ToString("D2") + ".obj");
                if (cloudPrefabs[i] == null) throw new InvalidOperationException("Bulut modeli eksik: " + i);
            }

            var oldNature = world.Find("Doğa Arka Planı");
            if (oldNature != null) UnityEngine.Object.DestroyImmediate(oldNature.gameObject);
            var oldCloudSea = world.Find(GroupName);
            if (oldCloudSea != null) UnityEngine.Object.DestroyImmediate(oldCloudSea.gameObject);
            var group = new GameObject(GroupName).transform;
            group.SetParent(world, false);
            var mountains = new GameObject("Dağ zirveleri").transform;
            mountains.SetParent(group, false);
            var rocks = new GameObject("Kaya çıkıntıları").transform;
            rocks.SetParent(group, false);
            var cloudBank = new GameObject("Bulut katmanları").transform;
            cloudBank.SetParent(group, false);

            EnsureFolder(Art, "Materials");
            var nearCloud = LitMaterial("Cloud_CloudWhite", new Color(0.97f, 0.99f, 1f), new Color(0.44f, 0.49f, 0.60f));
            var middleCloud = LitMaterial("Cloud_CloudLavender", new Color(0.95f, 0.94f, 1f), new Color(0.39f, 0.38f, 0.49f));
            var farCloud = LitMaterial("Cloud_CloudPeach", new Color(0.98f, 0.89f, 0.91f), new Color(0.42f, 0.33f, 0.37f));
            var nearMountain = LitMaterial("Cloud_MountainNear", new Color(0.25f, 0.30f, 0.45f), new Color(0.018f, 0.019f, 0.03f));
            var farMountain = LitMaterial("Cloud_MountainFar", new Color(0.38f, 0.38f, 0.57f), new Color(0.03f, 0.025f, 0.045f));
            var supportRock = LitMaterial("Cloud_SupportRock", new Color(0.36f, 0.34f, 0.40f), new Color(0.025f, 0.02f, 0.02f));

            AddSky(group);
            // Their bases sit well below the cloud surface. The house and its balancing rock stay in front.
            AddMountain(mountains, mountainPrefab, "Sol yakın zirve", -18f, 6.6f, 20.0f, 18f, nearMountain, 20f);
            AddMountain(mountains, mountainPrefab, "Sağ yakın zirve", 18f, 6.4f, 22.0f, 18f, nearMountain, -32f);
            AddMountain(mountains, mountainPrefab, "Sol uzak zirve", -34f, 8.6f, 36.0f, 23f, farMountain, 44f);
            AddMountain(mountains, mountainPrefab, "Orta uzak zirve", 2f, 7.2f, 45.0f, 21f, farMountain, -12f);
            AddMountain(mountains, mountainPrefab, "Sağ uzak zirve", 34f, 8.4f, 38.0f, 23f, farMountain, 31f);

            // Mountains near the foot of the house echo its existing rock silhouette.
            AddMountain(rocks, mountainPrefab, "Sol kaya çıkıntısı", -3.7f, -3.8f, 3.8f, 2.9f, supportRock, 28f);
            AddMountain(rocks, mountainPrefab, "Sağ kaya çıkıntısı", 3.8f, -4.0f, 4.1f, 3.0f, supportRock, -24f);

            var random = new System.Random(4817);
            AddCloudLayer(cloudBank, cloudPrefabs, farCloud, random, 18f, 0.1f, -43f, 43f, 4.5f, 10.2f, 1);
            AddCloudLayer(cloudBank, cloudPrefabs, middleCloud, random, 10f, -1.2f, -31f, 31f, 3.8f, 8.0f, 3);
            AddCloudLayer(cloudBank, cloudPrefabs, nearCloud, random, 5.5f, -3.0f, -22f, 22f, 3.0f, 7.0f, 5);

            var camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.04f, 0.07f, 0.20f);
            }
        }

        private static void AddCloudLayer(Transform parent, GameObject[] prefabs, Material material,
            System.Random random, float z, float y, float left, float right, float step, float width, int variantStart)
        {
            for (var x = left; x <= right; x += step)
            {
                var index = BankClouds[(variantStart + random.Next(BankClouds.Length)) % BankClouds.Length];
                var cloud = (GameObject)PrefabUtility.InstantiatePrefab(prefabs[index], parent);
                cloud.name = "Bulut " + Mathf.RoundToInt(z) + "m / " + index.ToString("D2");
                cloud.transform.localRotation = Quaternion.Euler(0f, (float)(random.NextDouble() * 28.0 - 14.0),
                    (float)(random.NextDouble() * 9.0 - 4.5));
                FitWidth(cloud.transform, width * (float)(0.78 + random.NextDouble() * 0.4));
                CenterAt(cloud.transform,
                    new Vector3(x + (float)(random.NextDouble() * 2.0 - 1.0),
                        y + (float)(random.NextDouble() * 0.85 - 0.35),
                        z + (float)(random.NextDouble() * 2.0 - 1.0)));
                Decorate(cloud, material);
            }
        }

        private static void AddMountain(Transform parent, GameObject prefab, string name,
            float x, float tip, float z, float height, Material material, float yaw)
        {
            var mountain = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            mountain.name = name;
            mountain.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            var bounds = BoundsOf(mountain);
            // FBX importers can put a large unit conversion on the model root.
            // Preserve that scale rather than replacing it with the fit factor.
            mountain.transform.localScale *= height / Mathf.Max(0.01f, bounds.size.y);
            bounds = BoundsOf(mountain);
            mountain.transform.position += new Vector3(x - bounds.center.x, tip - bounds.max.y, z - bounds.center.z);
            Decorate(mountain, material);
        }

        private static void FitWidth(Transform target, float width)
        {
            var bounds = BoundsOf(target.gameObject);
            target.localScale *= width / Mathf.Max(0.01f, bounds.size.x);
        }

        private static void CenterAt(Transform target, Vector3 center)
        {
            var bounds = BoundsOf(target.gameObject);
            target.position += center - bounds.center;
        }

        private static Bounds BoundsOf(GameObject root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Model has no renderer: " + root.name);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        private static void Decorate(GameObject root, Material material)
        {
            foreach (var renderer in root.GetComponentsInChildren<Renderer>())
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            foreach (var collider in root.GetComponentsInChildren<Collider>())
                UnityEngine.Object.DestroyImmediate(collider);
        }

        private static Material LitMaterial(string name, Color color, Color emission)
        {
            var path = Art + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader not found");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AddSky(Transform parent)
        {
            var imagePath = Art + "/CloudSeaSky.png";
            if (!File.Exists(imagePath))
            {
                const int width = 512, height = 512;
                var image = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (var y = 0; y < height; y++)
                {
                    var v = y / (float)(height - 1);
                    var color = Color.Lerp(new Color(0.90f, 0.63f, 0.66f),
                        new Color(0.025f, 0.065f, 0.22f), Mathf.SmoothStep(0f, 1f, v));
                    for (var x = 0; x < width; x++)
                    {
                        var u = x / (float)(width - 1);
                        var glow = Mathf.Exp(-((u - 0.79f) * (u - 0.79f) / 0.009f +
                            (v - 0.69f) * (v - 0.69f) / 0.018f));
                        var sun = Mathf.Exp(-((u - 0.79f) * (u - 0.79f) / 0.00032f +
                            (v - 0.69f) * (v - 0.69f) / 0.00032f));
                        image.SetPixel(x, y, Color.Lerp(color, new Color(1f, 0.75f, 0.53f), glow * 0.85f) +
                            new Color(sun, sun * 0.84f, sun * 0.65f, 0f));
                    }
                }
                image.Apply();
                File.WriteAllBytes(imagePath, image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
                AssetDatabase.ImportAsset(imagePath, ImportAssetOptions.ForceSynchronousImport);
            }
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
            var materialPath = Art + "/Materials/Cloud_Sky.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Unlit shader not found");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Cull", 0f);
            EditorUtility.SetDirty(material);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = "Alacakaranlık gökyüzü";
            quad.transform.SetParent(parent, false);
            quad.transform.localPosition = new Vector3(0f, -2.5f, 80f);
            quad.transform.localScale = new Vector3(160f, 90f, 1f);
            quad.GetComponent<MeshRenderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
