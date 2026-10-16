using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.Levels.Level1
{
    internal static class L1_AssetReviewBoard
    {
        private const string BoardName = "[L1] Asset Review Board";
        private const string CameraName = "[L1] Asset Review Camera";

        private static readonly (string label, string path)[] Samples =
        {
            ("TAVERN / window wall", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Wall with Window.prefab"),
            ("TAVERN / doorway", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Wall with Doorway.prefab"),
            ("TAVERN / straight wall", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Straight Wall.prefab"),
            ("TAVERN / side roof", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Side Roof.prefab"),
            ("TAVERN / floor", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Floor Tile.prefab"),
            ("TAVERN / support", "Assets/Low Poly Tarvern Pack/Prefabs/Building/Wall Support.prefab"),
            ("DOLL / window", "Assets/LowPolyInteriorsFree/Prefabs/02_Openings/SM_LPI_Open_Window_01.prefab"),
            ("DOLL / door", "Assets/LowPolyInteriorsFree/Prefabs/02_Openings/SM_LPI_Open_DoorSet_01.prefab"),
            ("DOLL / brick", "Assets/LowPolyInteriorsFree/Prefabs/01_Structure/SM_LPI_Struct_BrickWall_01.prefab"),
            ("DOLL / plank floor", "Assets/LowPolyInteriorsFree/Prefabs/22_Flooring/SM_LPI_Floor_PlankOak_01.prefab"),
            ("DOLL / bed", "Assets/LowPolyInteriorsFree/Prefabs/07_Bedroom/SM_LPI_Bed_Double_01.prefab"),
            ("DOLL / sofa", "Assets/LowPolyInteriorsFree/Prefabs/04_Seating/SM_LPI_Seat_Sofa_01.prefab"),
            ("DOLL / bookshelf", "Assets/LowPolyInteriorsFree/Prefabs/06_Storage/SM_LPI_Storage_Bookcase_03.prefab"),
            ("DOLL / kitchen", "Assets/LowPolyInteriorsFree/Prefabs/08_Kitchen/SM_LPI_Kitchen_Base_01.prefab"),
            ("DOLL / bath", "Assets/LowPolyInteriorsFree/Prefabs/12_Bathroom/SM_LPI_Bath_Tub_01.prefab"),
            ("DOLL / fireplace", "Assets/LowPolyInteriorsFree/Prefabs/21_Fireplace/SM_LPI_Fire_Surround_01.prefab"),
            ("3DIG / shelf", "Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/P_Shelf.prefab"),
            ("3DIG / plant", "Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/P_Plant.prefab"),
            ("FENCE / beam", "Assets/EmaceArt/Fences 2026/Prefabs/Fence/Beam/PRE/EA03_Fence_Beam_01a_PRE.prefab"),
            ("SEWER / plank", "Assets/Ata Khani/Modular Sewer Props/Prefabs/Wooden_Plank_01.prefab"),
            ("SEWER / scaffold", "Assets/Ata Khani/Modular Sewer Props/Prefabs/Scaffold_01.prefab"),
            ("TAVERN / chest", "Assets/Low Poly Tarvern Pack/Prefabs/Probs/Wooden Chest.prefab"),
            ("TAVERN / chair", "Assets/Low Poly Tarvern Pack/Prefabs/Probs/Backing Chair.prefab"),
            ("MIX / big sofa", "Assets/ZNS3D/Mix_Furniture_Pack/Meshes/big_sofa.fbx")
        };

        [MenuItem("Son Denge/Level1/Assetleri Yan Yana Goster")]
        private static void Show()
        {
            Clear();
            var board = new GameObject(BoardName);
            for (var i = 0; i < Samples.Length; i++)
            {
                var (label, path) = Samples[i];
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (source == null)
                {
                    Debug.LogWarning("Asset yok: " + path);
                    continue;
                }
                var column = i % 6;
                var row = i / 6;
                var position = new Vector3((column - 2.5f) * 2.55f, (1.5f - row) * 2.23f + 0.28f, -50f);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, board.transform);
                instance.name = label;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                var renderers = instance.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) continue;
                var bounds = renderers[0].bounds;
                for (var j = 1; j < renderers.Length; j++) bounds.Encapsulate(renderers[j].bounds);
                var factor = Mathf.Min(1.88f / Mathf.Max(bounds.size.x, .01f),
                    1.48f / Mathf.Max(bounds.size.y, .01f), 1.40f / Mathf.Max(bounds.size.z, .01f));
                instance.transform.localScale = Vector3.one * factor;
                bounds = renderers[0].bounds;
                for (var j = 1; j < renderers.Length; j++) bounds.Encapsulate(renderers[j].bounds);
                instance.transform.position += position - bounds.center;

                var caption = new GameObject("Label " + i);
                caption.transform.SetParent(board.transform, false);
                caption.transform.position = new Vector3(position.x, position.y - 1.01f, -51.0f);
                var text = caption.AddComponent<TextMesh>();
                text.text = label;
                text.anchor = TextAnchor.MiddleCenter;
                text.alignment = TextAlignment.Center;
                text.fontSize = 60;
                text.characterSize = 0.045f;
                text.color = new Color(.95f, .90f, .77f);
            }

            var cameraObject = new GameObject(CameraName);
            var camera = cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0, 0, -64f);
            camera.orthographic = true;
            camera.orthographicSize = 5.1f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 29f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.13f, .17f, .21f);
            camera.enabled = true;
            Debug.Log("L1 asset inceleme tahtası hazır: " + Samples.Length + " örnek.");
        }

        [MenuItem("Son Denge/Level1/Asset Inceleme Tahtasini Kaldir")]
        private static void Clear()
        {
            var board = SceneRoot(BoardName);
            if (board != null) Object.DestroyImmediate(board);
            var camera = SceneRoot(CameraName);
            if (camera != null) Object.DestroyImmediate(camera);
        }

        private static GameObject SceneRoot(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }
    }
}
