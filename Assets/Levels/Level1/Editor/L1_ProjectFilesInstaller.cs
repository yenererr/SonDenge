using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.Levels.Level1
{
    internal static class L1_ProjectFilesInstaller
    {
        private const string Art = "Assets/Levels/Level1/Art/ProjectFiles/";
        private const string MaterialPath = "Assets/Levels/Level1/Materials/L1_ProjectSprite.mat";

        [MenuItem("Son Denge/Level1/Proje Dosyası Mekaniklerini 3D Eve Uygula")]
        public static void Install() => Apply(true);

        [MenuItem("Son Denge/Level1/Proje Dosyası Mekaniklerini 3D Eve Uygula", true)]
        private static bool CanInstall() => !EditorApplication.isPlayingOrWillChangePlaymode;

        public static void Apply(bool saveScene)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Önce Play modunu durdurun.");
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Önce Level1 sahnesini açın.");

            EnsureEquipmentIcons();
            ImportSprites();
            var material = EnsureMaterial();
            var worldObject = GameObject.Find("[L1] World Visuals");
            if (worldObject == null) throw new InvalidOperationException("Önce oynanabilir 3D evi kurun.");
            var world = worldObject.transform;
            var house = world.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name.StartsWith("3D Ev"));
            if (house == null) throw new InvalidOperationException("3D ev bulunamadı.");

            var pivot = Child(world, "[L1] Denge merkezi");
            pivot.position = new Vector3(0f, -1.67f, 0f);
            if (house.parent != pivot) house.SetParent(pivot, true);

            var oldBackdrop = house.Find("Proje dosyası — resimli oda arka planları");
            if (oldBackdrop != null) UnityEngine.Object.DestroyImmediate(oldBackdrop.gameObject);
            ConfigureExpandableThirdFloor(house);
            var camera = Camera.main;
            if (camera != null)
            {
                var position = camera.transform.position;
                camera.transform.position = new Vector3(position.x, 1.65f, position.z);
                camera.orthographicSize = 4.1f;
                EditorUtility.SetDirty(camera);
            }

            var drag = world.GetComponentInChildren<L1_PropDrag>(true);
            var resident = house.GetComponentInChildren<L1_ResidentController>(true);
            var rules = world.GetComponentInChildren<L1_ProjectGameController>(true);
            if (rules == null) rules = Child(world, "[L1] Beş afet oyunu").gameObject.AddComponent<L1_ProjectGameController>();
            rules.houseRoot = house;
            rules.balancePivot = pivot;
            rules.resident = resident;
            rules.drag = drag;
            rules.projectSpriteMaterial = material;
            rules.spriteEntries = Directory.GetFiles(Art, "*.png")
                .Select(path => new L1_ProjectSpriteEntry
                {
                    id = Path.GetFileNameWithoutExtension(path),
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path.Replace('\\', '/'))
                }).Where(entry => entry.sprite != null).ToArray();
            rules.planningMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Levels/Level1/Audio/bgm_cozy_mountain.mp3");
            rules.disasterMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Levels/Level1/Audio/bgm_disaster_tension.mp3");
            EditorUtility.SetDirty(rules);

            if (drag != null)
            {
                drag.houseRoot = house;
                drag.canDrag = true;
                EditorUtility.SetDirty(drag);
            }

            SetFurnitureMass(house, "buzdolabı", 90f);
            SetFurnitureMass(house, "ocak ve fırın", 75f);
            SetFurnitureMass(house, "banyo lavabosu", 25f);
            SetFurnitureMass(house, "küvet", 50f);
            SetFurnitureMass(house, "kanepe", 60f);
            SetFurnitureMass(house, "tekli koltuk", 35f);
            SetFurnitureMass(house, "sehpa", 20f);
            SetFurnitureMass(house, "televizyon", 30f);
            SetFurnitureMass(house, "yatak", 70f);
            SetFurnitureMass(house, "Komodin", 20f);
            SetFurnitureMass(house, "kitaplık", 85f);
            SetFurnitureMass(house, "sandığı", 25f);

            EditorSceneManager.MarkSceneDirty(scene);
            if (saveScene) EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("project-files afet mekanikleri ve geçici 2D eşya kaynakları 3D Level1 evine uygulandı.");
        }

        private static void ImportSprites()
        {
            foreach (var path in Directory.GetFiles(Art, "*.png"))
            {
                var assetPath = path.Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer == null) { AssetDatabase.ImportAsset(assetPath); importer = AssetImporter.GetAtPath(assetPath) as TextureImporter; }
                if (importer == null) continue;
                if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single && !importer.mipmapEnabled) continue;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100f;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureEquipmentIcons()
        {
            var ids = new[] { "storm_brace", "lightning_rod_system", "surge_protector", "gutter_system",
                "drainage_channel", "waterproof_sealing", "sandbags", "water_pump", "water_sensor" };
            foreach (var id in ids)
            {
                var path = Art + id + ".png";
                if (File.Exists(path)) continue;
                var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                var pixels = new Color32[128 * 128];
                for (var i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 0);
                texture.SetPixels32(pixels);
                var amber = new Color32(218, 159, 83, 255);
                var pale = new Color32(252, 225, 170, 255);
                var blue = new Color32(84, 179, 209, 255);
                var red = new Color32(205, 98, 77, 255);
                // Hand-sized, transparent pictograms for equipment missing from the supplied art pack.
                DrawLine(texture, 18, 24, 110, 24, amber, 5);
                DrawLine(texture, 18, 24, 18, 106, amber, 5);
                DrawLine(texture, 18, 106, 110, 106, amber, 5);
                DrawLine(texture, 110, 106, 110, 24, amber, 5);
                if (id == "storm_brace")
                {
                    DrawLine(texture, 35, 38, 93, 92, pale, 9);
                    DrawLine(texture, 35, 92, 93, 38, pale, 9);
                }
                else if (id == "lightning_rod_system")
                {
                    DrawLine(texture, 64, 38, 64, 91, amber, 8);
                    DrawLine(texture, 64, 91, 54, 75, pale, 7);
                    DrawLine(texture, 64, 91, 74, 75, pale, 7);
                    DrawLine(texture, 37, 36, 91, 36, blue, 6);
                }
                else if (id == "surge_protector")
                {
                    DrawLine(texture, 42, 88, 86, 88, pale, 7);
                    DrawLine(texture, 42, 88, 42, 48, pale, 7);
                    DrawLine(texture, 86, 88, 86, 48, pale, 7);
                    DrawLine(texture, 42, 48, 86, 48, pale, 7);
                    DrawLine(texture, 69, 80, 55, 66, blue, 6);
                    DrawLine(texture, 55, 66, 74, 59, blue, 6);
                    DrawLine(texture, 74, 59, 60, 40, blue, 6);
                }
                else if (id == "gutter_system" || id == "drainage_channel")
                {
                    DrawLine(texture, 34, 77, 94, 77, amber, 8);
                    DrawLine(texture, 34, 77, 34, 54, amber, 8);
                    DrawLine(texture, 94, 77, 94, 54, amber, 8);
                    DrawLine(texture, 42, 61, 86, 61, blue, 8);
                    if (id == "drainage_channel") DrawLine(texture, 64, 62, 64, 34, blue, 7);
                }
                else if (id == "waterproof_sealing" || id == "water_sensor")
                {
                    DrawLine(texture, 64, 93, 39, 58, blue, 9);
                    DrawLine(texture, 39, 58, 64, 35, blue, 9);
                    DrawLine(texture, 64, 35, 89, 58, blue, 9);
                    DrawLine(texture, 89, 58, 64, 93, blue, 9);
                    if (id == "water_sensor") DrawLine(texture, 43, 98, 85, 98, pale, 5);
                }
                else if (id == "sandbags")
                {
                    for (var row = 0; row < 3; row++)
                        for (var col = 0; col < 3 - row; col++)
                            DrawLine(texture, 35 + col * 24 + row * 12, 43 + row * 20,
                                53 + col * 24 + row * 12, 43 + row * 20, pale, 14);
                }
                else if (id == "water_pump")
                {
                    DrawLine(texture, 37, 43, 89, 43, amber, 9);
                    DrawLine(texture, 37, 43, 37, 86, amber, 9);
                    DrawLine(texture, 89, 43, 89, 86, amber, 9);
                    DrawLine(texture, 37, 86, 89, 86, amber, 9);
                    DrawLine(texture, 48, 65, 80, 65, blue, 11);
                }
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void DrawLine(Texture2D texture, int x1, int y1, int x2, int y2, Color32 color, int width)
        {
            var length = Mathf.Max(Mathf.Abs(x2 - x1), Mathf.Abs(y2 - y1));
            for (var step = 0; step <= length; step++)
            {
                var x = Mathf.RoundToInt(Mathf.Lerp(x1, x2, step / (float)Mathf.Max(1, length)));
                var y = Mathf.RoundToInt(Mathf.Lerp(y1, y2, step / (float)Mathf.Max(1, length)));
                for (var yy = -width / 2; yy <= width / 2; yy++)
                    for (var xx = -width / 2; xx <= width / 2; xx++)
                        if (xx * xx + yy * yy <= width * width / 4 && x + xx >= 0 && x + xx < 128 && y + yy >= 0 && y + yy < 128)
                            texture.SetPixel(x + xx, y + yy, color);
            }
        }

        private static Material EnsureMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var shader = Shader.Find("SonDenge/ProjectSprite");
            if (shader == null) throw new InvalidOperationException("ProjectSprite shader derlenmedi.");
            if (material == null)
            {
                material = new Material(shader) { name = "L1_ProjectSprite" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else if (material.shader != shader) { material.shader = shader; EditorUtility.SetDirty(material); }
            return material;
        }

        private static Transform Child(Transform parent, string name)
        {
            var existing = parent.Find(name);
            if (existing != null) return existing;
            var node = new GameObject(name).transform;
            node.SetParent(parent, false);
            return node;
        }

        private static void ConfigureExpandableThirdFloor(Transform house)
        {
            var upper = house.Find("Kat 3");
            var upperLadder = house.Find("Orta kattan çatı katına");
            var roof = house.Find("Asimetrik kiremit çatı");
            var balcony = house.Find("Ahşap balkon ve vinç");
            var extras = Child(house, "[L1] Üçüncü kat balkon ve vinç parçaları");
            if (balcony != null)
            {
                foreach (Transform part in balcony.Cast<Transform>().ToArray())
                    if (part.localPosition.y >= 2.5f) part.SetParent(extras, true);
            }
            if (upper != null) upper.gameObject.SetActive(false);
            if (upperLadder != null) upperLadder.gameObject.SetActive(false);
            extras.gameObject.SetActive(false);
            if (roof != null) roof.localPosition = new Vector3(0f, -2.25f, 0f);
            foreach (var prop in house.GetComponentsInChildren<L1_MovableProp>(true))
                if (house.InverseTransformPoint(prop.transform.position).y >= 2.5f)
                    prop.gameObject.SetActive(false);
        }

        private static void SetFurnitureMass(Transform house, string namePart, float mass)
        {
            foreach (var prop in house.GetComponentsInChildren<L1_MovableProp>(true))
            {
                if (prop.displayName == null || !prop.displayName.Contains(namePart)) continue;
                var body = prop.GetComponent<Rigidbody>();
                if (body == null) continue;
                body.mass = mass;
                EditorUtility.SetDirty(body);
            }
        }
    }
}
