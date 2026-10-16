using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Reference artwork with separate 3D collision and climb geometry.</summary>
    internal static class L1_ReferenceHouseBuilder
    {
        private const string Art = "Assets/Levels/Level1/Art/";
        private const float UnitsPerPixel = 0.006f;
        private const float ImageWidth = 1086f;
        private const float ImageHeight = 1448f;
        private const float PivotPixelY = 1350f;

        [MenuItem("Son Denge/Level1/Gorsel Ev ve Dagi Kur")]
        public static void Build()
        {
            BuildHouse(File.Exists(Art + "L1_ReferenceHouse_Furnished.png")
                ? "L1_ReferenceHouse_Furnished.png" : "L1_ReferenceHouse_Empty.png");
        }

        [MenuItem("Son Denge/Level1/Referanstaki Bos Evi Goster")]
        private static void BuildOriginal() => BuildHouse("L1_ReferenceHouse_Empty.png");

        private static void BuildHouse(string file)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Önce Level1 sahnesini açın.");

            ImportHouse(Art + file);
            var old = Root("[L1] World Visuals");
            if (old != null) UnityEngine.Object.DestroyImmediate(old);

            var world = new GameObject("[L1] World Visuals");
            var sky = Sprite(world.transform, "Dusk sky", Art + "L1_DuskSky.png",
                new Vector3(0f, 2f, 9f), Vector3.one * 2.05f, -20);
            sky.color = new Color(0.70f, 0.76f, 0.91f, 1f);
            var rock = Sprite(world.transform, "Narrow mountain summit", Art + "L1_RockSpire.png",
                new Vector3(0f, -3.85f, 0.6f), Vector3.one * 0.32f, -10);
            rock.color = new Color(0.88f, 0.91f, 1f, 1f);

            var pivot = Child(world.transform, "House pivot - rotate this for balance");
            pivot.localPosition = new Vector3(0f, -1.3f, 0f);
            var facade = Sprite(pivot, "Reference house - detailed illustrated facade", Art + file,
                Vector3.zero, Vector3.one, 0);
            // Keep the original image proportions even if an edited variant has
            // a different export resolution. All physics uses reference pixels.
            facade.transform.localScale = new Vector3(
                ImageWidth * UnitsPerPixel / facade.sprite.bounds.size.x,
                ImageHeight * UnitsPerPixel / facade.sprite.bounds.size.y, 1f);

            var collision = Child(pivot, "3D floors aligned to artwork");
            Floor(collision, "Ground floor", new Vector2(120f, 1260f), new Vector2(980f, 1270f));
            Floor(collision, "Middle floor left", new Vector2(110f, 946f), new Vector2(400f, 942f));
            Floor(collision, "Middle floor centre", new Vector2(500f, 941f), new Vector2(575f, 940f));
            Floor(collision, "Middle floor right", new Vector2(675f, 939f), new Vector2(955f, 935f));
            Floor(collision, "Upper floor left", new Vector2(95f, 625f), new Vector2(450f, 606f));
            Floor(collision, "Upper floor right", new Vector2(550f, 600f), new Vector2(940f, 579f));

            var ladders = Child(pivot, "Katlar arasi gecisler");
            Ladder(ladders, "Alt kat sol merdiven", new Vector2(450f, 1264f),
                new Vector2(450f, 942f), new Vector2(360f, 943f));
            Ladder(ladders, "Alt kat sag merdiven", new Vector2(625f, 1266f),
                new Vector2(625f, 940f), new Vector2(720f, 938f));
            Ladder(ladders, "Orta kat ust merdiven", new Vector2(505f, 940f),
                new Vector2(505f, 603f), new Vector2(605f, 597f));

            foreach (var point in new[]
            {
                new Vector2(525f, 394f), new Vector2(406f, 741f),
                new Vector2(596f, 712f), new Vector2(335f, 1053f),
                new Vector2(744f, 1040f)
            })
            {
                var lamp = Child(pivot, "Amber reference lantern");
                lamp.localPosition = P(point) + new Vector3(0f, 0f, -0.35f);
                var light = lamp.gameObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.56f, 0.24f);
                light.intensity = 0.7f;
                light.range = 1.8f;
                light.shadows = LightShadows.None;
            }

            var camera = Root("Main Camera")?.GetComponent<Camera>();
            if (camera != null)
            {
                camera.transform.SetPositionAndRotation(new Vector3(0f, 2f, -14f), Quaternion.identity);
                camera.orthographic = false;
                camera.fieldOfView = 40f;
            }
            var sun = Root("Directional Light")?.GetComponent<Light>();
            if (sun != null)
            {
                sun.color = new Color(1f, 0.82f, 0.66f);
                sun.intensity = 0.5f;
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Level1 referans ev kuruldu: 3 kat, ayrıntılı şeffaf görsel, 3D zemin ve merdiven alanları.");
        }

        private static void ImportHouse(string path)
        {
            AssetDatabase.ImportAsset(path);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Ev görseli bulunamadı: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1f / UnitsPerPixel;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 1f - PivotPixelY / ImageHeight);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private static Vector3 P(Vector2 pixel) => new Vector3(
            (pixel.x - ImageWidth * 0.5f) * UnitsPerPixel,
            (PivotPixelY - pixel.y) * UnitsPerPixel, -0.3f);

        private static void Floor(Transform parent, string name, Vector2 a, Vector2 b)
        {
            var start = P(a);
            var end = P(b);
            var floor = Child(parent, name);
            var delta = end - start;
            floor.localPosition = (start + end) * 0.5f - Vector3.up * 0.04f;
            floor.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            var collider = floor.gameObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(delta.magnitude, 0.08f, 0.9f);
        }

        private static void Ladder(Transform parent, string name, Vector2 low, Vector2 high, Vector2 exit)
        {
            var bottom = Child(parent, name + " alt");
            bottom.localPosition = P(low) + Vector3.up * 0.4f;
            var top = Child(parent, name + " ust");
            top.localPosition = P(high) + Vector3.up * 0.4f;
            var landing = Child(parent, name + " cikis");
            landing.localPosition = P(exit) + Vector3.up * 0.4f;
            var zone = Child(parent, name);
            zone.localPosition = (top.localPosition + bottom.localPosition) * 0.5f;
            var trigger = zone.gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(0.64f, Vector3.Distance(top.position, bottom.position) + 0.6f, 0.9f);
            var climb = zone.gameObject.AddComponent<L1_LadderClimb>();
            climb.bottom = bottom;
            climb.top = top;
            climb.upperLanding = landing;
            climb.climbSpeed = 1.7f;
        }

        private static Transform Child(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            return obj.transform;
        }

        private static SpriteRenderer Sprite(Transform parent, string name, string path,
            Vector3 position, Vector3 scale, int order)
        {
            var child = Child(parent, name);
            child.localPosition = position;
            child.localScale = scale;
            var renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (renderer.sprite == null) throw new InvalidOperationException("Sprite bulunamadı: " + path);
            renderer.sortingOrder = order;
            return renderer;
        }

        private static GameObject Root(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        [MenuItem("Son Denge/Level1/Referans Ev Onizleme Kaydet")]
        private static void Capture()
        {
            var camera = Root("Main Camera")?.GetComponent<Camera>();
            if (camera == null) return;
            var previous = camera.targetTexture;
            var active = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1920, 1080, 24);
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                File.WriteAllBytes(Art + "L1_ReferenceHouse_Preview.png", image.EncodeToPNG());
                AssetDatabase.ImportAsset(Art + "L1_ReferenceHouse_Preview.png");
            }
            finally
            {
                camera.targetTexture = previous;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(target);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
