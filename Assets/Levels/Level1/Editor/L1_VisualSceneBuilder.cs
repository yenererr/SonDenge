using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.Levels.Level1
{
    internal static class L1_VisualSceneBuilder
    {
        private const string LevelRoot = "Assets/Levels/Level1";
        private const string Tavern = "Assets/Low Poly Tarvern Pack/Prefabs/Building/";
        private const string TavernProps = "Assets/Low Poly Tarvern Pack/Prefabs/Probs/";
        private const string Interiors = "Assets/LowPolyInteriorsFree/Prefabs/01_Structure/";
        private const string Doll = "Assets/LowPolyInteriorsFree/Prefabs/";
        private const string SmallProps = "Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/";

        [MenuItem("Son Denge/Level1/Gorsel Ev ve Dagi Kur")]
        private static void Build()
        {
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Level1.unity")
            {
                Debug.LogError("L1 görsel kurucu yalnız Assets/Scenes/Level1.unity içinde çalışır.");
                return;
            }

            EnsureFolder(LevelRoot + "/Materials");
            var plaster = Material("L1_WarmPlaster", new Color(0.80f, 0.66f, 0.49f));
            var timber = Material("L1_DarkTimber", new Color(0.26f, 0.13f, 0.075f));
            var warmWood = Material("L1_WarmWood", new Color(0.48f, 0.25f, 0.11f));
            var planks = Material("L1_OakPlanks", new Color(0.86f, 0.72f, 0.56f));
            planks.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(
                LevelRoot + "/Art/L1_OakFloor_Diffuse.tga"));
            var roof = Material("L1_RoofClay", new Color(0.49f, 0.21f, 0.13f));
            var stone = Material("L1_PivotStone", new Color(0.40f, 0.37f, 0.34f));
            var olive = Material("L1_OliveFabric", new Color(0.30f, 0.39f, 0.19f));
            var cream = Material("L1_CreamCloth", new Color(0.87f, 0.73f, 0.53f));
            var glass = Material("L1_WindowGlass", new Color(0.38f, 0.62f, 0.68f));

            ImportSprite(LevelRoot + "/Art/L1_DuskSky.png", false);
            ImportSprite(LevelRoot + "/Art/L1_RockSpire.png", true);

            var existing = SceneRoot("[L1] World Visuals");
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            var world = new GameObject("[L1] World Visuals");
            var backdrop = Child(world.transform, "Illustrated dusk and mountain");
            Sprite(backdrop, "Dusk sky", LevelRoot + "/Art/L1_DuskSky.png", new Vector3(0, 2.0f, 9f), 2.05f, -20);
            Sprite(backdrop, "Rock spire", LevelRoot + "/Art/L1_RockSpire.png", new Vector3(0, -3.12f, 2.2f), 0.49f, -10);

            var pivot = Child(world.transform, "House pivot - rotate this for balance");
            pivot.transform.position = new Vector3(0f, 0.15f, 0f);
            var house = Child(pivot.transform, "Two-floor cutaway house");

            // Four open-front rooms, each about four metres wide. Asset prefabs provide
            // the visible walls, floors, windows and timber structure.
            for (var floor = 0; floor < 2; floor++)
            {
                var baseY = floor * 2.65f;
                for (var side = -1; side <= 1; side += 2)
                {
                    var x = side * 2.05f;
                    var doorway = floor == 0 && side > 0;
                    var wall = FitPrefab(Tavern + (doorway ? "Wall with Doorway.prefab" : "Wall with Window.prefab"),
                        house.transform, $"Floor {floor + 1} {Side(side)} tavern wall",
                        new Vector3(x, baseY + 1.39f, 0.82f), new Vector3(3.87f, 2.45f, 0.22f));
                    if (!doorway && side < 0)
                    {
                        // The shutter is a separate prefab child; opening it makes the
                        // illustrated mountain sky visible through the room.
                        var shutter = wall.transform.Find("Window");
                        if (shutter != null) shutter.gameObject.SetActive(false);
                    }
                    FitPrefab(Tavern + "Floor Tile.prefab", house.transform,
                        $"Floor {floor + 1} {Side(side)} tavern boards", new Vector3(x, baseY + 0.16f, -0.05f),
                        new Vector3(4.03f, 0.18f, 1.90f));
                    if (!doorway)
                    {
                        for (var strip = 0; strip < 3; strip++)
                            Block(house.transform, $"Floor {floor + 1} {Side(side)} oak wall board {strip}",
                                new Vector3(x, baseY + 0.42f + strip * 0.18f, 0.68f),
                                new Vector3(3.63f, 0.16f, 0.055f), planks);
                    }
                    Block(house.transform, $"Floor {floor + 1} {Side(side)} lower beam",
                        new Vector3(x, baseY + 0.02f, -0.51f), new Vector3(4.12f, 0.18f, 0.30f), timber);
                    Block(house.transform, $"Floor {floor + 1} {Side(side)} lintel",
                        new Vector3(x, baseY + 2.57f, -0.54f), new Vector3(4.12f, 0.16f, 0.29f), warmWood);
                }
            }

            // Strong front frame makes the two floors and four rooms legible in silhouette.
            foreach (var x in new[] { -4.16f, 0f, 4.16f })
            {
                var post = FitPrefab(Interiors + "SM_LPI_Struct_Column_01.prefab", house.transform,
                    $"Carved front post {x}", new Vector3(x, 2.69f, -0.56f), new Vector3(0.29f, 5.49f, 0.30f));
                foreach (var renderer in post.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = warmWood;
            }
            foreach (var y in new[] { 0.09f, 2.70f, 5.37f })
            {
                FitPrefab(Interiors + "SM_LPI_Struct_Beam_01.prefab", house.transform,
                    $"Front timber beam {y}", new Vector3(0, y, -0.66f), new Vector3(8.65f, 0.20f, 0.30f));
            }

            // Warm plaster side cheeks give depth without closing the dollhouse front.
            foreach (var x in new[] { -4.24f, 4.24f })
            {
                for (var floor = 0; floor < 2; floor++)
                    Block(house.transform, $"Side plaster {x} {floor}",
                        new Vector3(x, floor * 2.65f + 1.40f, 0.15f),
                        new Vector3(0.13f, 2.34f, 1.35f), plaster);
            }

            // A filled gable and the original timber roof parts give the cutaway
            // a complete house silhouette rather than an empty triangular frame.
            var gable = Child(house.transform, "Warm plaster attic gable");
            var gableFilter = gable.AddComponent<MeshFilter>();
            gableFilter.sharedMesh = GableMesh();
            gable.AddComponent<MeshRenderer>().sharedMaterial = plaster;
            Block(house.transform, "Attic window glass", new Vector3(0, 5.90f, 0.53f),
                new Vector3(0.69f, 0.55f, 0.07f), glass);
            Block(house.transform, "Attic window upper frame", new Vector3(0, 6.22f, 0.46f),
                new Vector3(0.85f, 0.10f, 0.14f), timber);
            Block(house.transform, "Attic window lower frame", new Vector3(0, 5.59f, 0.46f),
                new Vector3(0.85f, 0.10f, 0.14f), timber);
            foreach (var x in new[] { -0.40f, 0.40f })
                Block(house.transform, "Attic window side frame", new Vector3(x, 5.90f, 0.46f),
                    new Vector3(0.10f, 0.68f, 0.14f), timber);
            Block(house.transform, "Attic window mullion", new Vector3(0, 5.90f, 0.43f),
                new Vector3(0.07f, 0.58f, 0.12f), timber);
            RoofBeam(house.transform, "Left roof slope", new Vector3(-2.18f, 5.94f, 0.08f), 18.5f, 4.65f, roof);
            RoofBeam(house.transform, "Right roof slope", new Vector3(2.18f, 5.94f, 0.08f), -18.5f, 4.65f, roof);
            FitPrefab(Tavern + "Side Roof.prefab", house.transform, "Tavern left timber roof",
                new Vector3(-2.18f, 5.90f, -0.78f), new Vector3(4.72f, 1.42f, 0.20f));
            var rightRoof = FitPrefab(Tavern + "Side Roof.prefab", house.transform, "Tavern right timber roof",
                new Vector3(2.18f, 5.90f, -0.78f), new Vector3(4.72f, 1.42f, 0.20f));
            rightRoof.transform.localScale = new Vector3(-rightRoof.transform.localScale.x,
                rightRoof.transform.localScale.y, rightRoof.transform.localScale.z);
            CenterPrefab(rightRoof, house.transform.TransformPoint(new Vector3(2.18f, 5.90f, -0.78f)));
            FitPrefab(Tavern + "Line Roof.prefab", house.transform, "Tavern roof detailing",
                new Vector3(0, 6.65f, 0.24f), new Vector3(1.22f, 0.22f, 1.45f));
            var chimney = FitPrefab(Interiors + "SM_LPI_Struct_Chimney_01.prefab", house.transform,
                "Stone chimney", new Vector3(-3.18f, 6.65f, 0.52f), new Vector3(0.56f, 1.50f, 0.56f));
            foreach (var renderer in chimney.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = stone;
            FitPrefab(Interiors + "SM_LPI_Struct_BrickWall_01.prefab", house.transform,
                "Brickwork on chimney face", new Vector3(-3.18f, 6.63f, 0.19f),
                new Vector3(0.52f, 1.38f, 0.08f));
            Block(house.transform, "Chimney cap", new Vector3(-3.18f, 7.42f, 0.52f),
                new Vector3(0.72f, 0.15f, 0.71f), stone);

            // The overhanging landing, rough fence and hanging delivery chest
            // echo the reference image while leaving the playable rooms clear.
            FitPrefab(Tavern + "Floor Tile.prefab", house.transform, "Right wooden balcony deck",
                new Vector3(4.94f, 2.73f, -0.08f), new Vector3(1.67f, 0.17f, 1.45f));
            FitPrefab("Assets/EmaceArt/Fences 2026/Prefabs/Fence/Beam/PRE/EA03_Fence_Beam_01a_PRE.prefab",
                house.transform, "Rough balcony rail", new Vector3(4.94f, 3.14f, -0.77f),
                new Vector3(1.64f, 0.72f, 0.20f));
            RoofBeam(house.transform, "Angled balcony brace", new Vector3(4.65f, 2.26f, 0.18f),
                35f, 1.60f, timber);
            Block(house.transform, "Delivery crane beam", new Vector3(5.02f, 5.20f, -0.10f),
                new Vector3(2.25f, 0.19f, 0.23f), timber);
            RoofBeam(house.transform, "Delivery crane brace", new Vector3(4.89f, 4.76f, -0.10f),
                30f, 1.32f, timber);
            Block(house.transform, "Hanging rope", new Vector3(6.02f, 4.65f, -0.10f),
                new Vector3(0.035f, 1.15f, 0.035f), warmWood);
            FitPrefab(TavernProps + "Wooden Chest.prefab", house.transform, "Hanging tavern chest",
                new Vector3(6.02f, 3.90f, -0.10f), new Vector3(0.69f, 0.62f, 0.58f));

            FitPrefab(Tavern + "Floor Tile.prefab", house.transform, "Left lower lookout deck",
                new Vector3(-4.77f, 0.16f, -0.09f), new Vector3(1.19f, 0.17f, 1.37f));
            FitPrefab("Assets/EmaceArt/Fences 2026/Prefabs/Fence/Beam/PRE/EA03_Fence_Beam_01a_PRE.prefab",
                house.transform, "Left lookout fence", new Vector3(-4.77f, 0.57f, -0.75f),
                new Vector3(1.15f, 0.65f, 0.19f));
            SlimBrace(house.transform, "Lookout diagonal support", new Vector3(-4.43f, -0.17f, 0.15f),
                -29f, 0.91f, timber);

            DressRooms(house.transform, olive, cream);

            // Structural platform and exposed round balancing pin are separate from the rock.
            Block(house.transform, "Weathered house platform", new Vector3(0, -0.13f, 0),
                new Vector3(8.82f, 0.26f, 2.05f), planks);
            SlimBrace(house.transform, "Left balance brace", new Vector3(-0.78f, -0.30f, 0.19f),
                -20f, 1.63f, timber);
            SlimBrace(house.transform, "Right balance brace", new Vector3(0.78f, -0.30f, 0.19f),
                20f, 1.63f, timber);
            var pin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pin.name = "Visible balancing pin";
            pin.transform.SetParent(world.transform, false);
            pin.transform.position = new Vector3(0f, 0.03f, -0.98f);
            pin.transform.rotation = Quaternion.Euler(90f, 0, 0);
            pin.transform.localScale = new Vector3(0.40f, 0.14f, 0.40f);
            pin.GetComponent<Renderer>().sharedMaterial = stone;

            for (var floor = 0; floor < 2; floor++)
            {
                for (var side = -1; side <= 1; side += 2)
                {
                    var lamp = Child(house.transform, $"Warm room light {floor} {side}");
                    lamp.transform.localPosition = new Vector3(side * 1.72f, floor * 2.65f + 2.16f, -0.14f);
                    FitPrefab("Assets/LowPolyInteriorsFree/Prefabs/13_Lighting/SM_LPI_Light_Lantern_01.prefab",
                        lamp.transform, "Hanging asset lantern", Vector3.zero,
                        new Vector3(0.25f, 0.38f, 0.25f));
                    var light = lamp.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.color = new Color(1f, 0.67f, 0.36f);
                    light.intensity = 1.36f;
                    light.range = 3.1f;
                    light.shadows = LightShadows.None;
                }
            }

            var daylight = SceneRoot("Directional Light")?.GetComponent<Light>();
            if (daylight != null)
            {
                daylight.color = new Color(1f, 0.82f, 0.68f);
                daylight.intensity = 0.66f;
            }

            pivot.transform.localScale = Vector3.one * 0.82f;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("L1 görsel ev ve dağ kuruldu; iki kat, dört oda, asset duvar/zemin ve açık pivot hazır.");
        }

        private static string Side(int side) => side < 0 ? "left" : "right";

        private static GameObject SceneRoot(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        private static void DressRooms(Transform house, Material olive, Material cream)
        {
            var rooms = Child(house, "Room dressing - visual assets").transform;

            for (var floor = 0; floor < 2; floor++)
            {
                foreach (var x in new[] { -3.12f, -0.98f })
                {
                    var curtain = FitPrefab(Doll + "16_Textiles/SM_LPI_Text_Curtain_01.prefab", rooms,
                        $"Open window curtain {floor} {x}",
                        new Vector3(x, floor * 2.65f + 1.40f, 0.61f),
                        new Vector3(0.25f, 0.92f, 0.15f));
                    foreach (var renderer in curtain.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = cream;
                }
            }

            // Lower left: the tiny kitchen and wash corner in the reference.
            FitPrefab(Doll + "12_Bathroom/SM_LPI_Bath_Tub_01.prefab", rooms, "Enamel bathtub",
                new Vector3(-3.34f, 0.57f, -0.14f), new Vector3(1.16f, 0.57f, 0.70f));
            FitPrefab(TavernProps + "Bar Counter.prefab", rooms, "Timber kitchen counter",
                new Vector3(-1.33f, 0.67f, 0.13f), new Vector3(1.47f, 0.84f, 0.64f));
            FitPrefab(TavernProps + "Standard Stool.prefab", rooms, "Kitchen stool",
                new Vector3(-1.36f, 0.47f, -0.52f), new Vector3(0.39f, 0.45f, 0.38f));
            FitPrefab(SmallProps + "P_Shelf.prefab", rooms, "Kitchen wall shelf",
                new Vector3(-2.77f, 1.92f, 0.54f), new Vector3(1.18f, 0.15f, 0.29f));
            FitPrefab(SmallProps + "P_Plant.prefab", rooms, "Kitchen herb pot",
                new Vector3(-2.91f, 2.14f, 0.50f), new Vector3(0.31f, 0.39f, 0.30f));
            FitPrefab(TavernProps + "Fat Mead Jug.prefab", rooms, "Kitchen jug",
                new Vector3(-1.03f, 1.23f, 0.09f), new Vector3(0.22f, 0.29f, 0.22f));

            // Lower right: a quiet sitting room with a green couch and bookcase.
            var sofa = FitPrefab(Doll + "04_Seating/SM_LPI_Seat_Sofa_01.prefab", rooms,
                "Olive sitting-room sofa", new Vector3(1.30f, 0.72f, -0.23f),
                new Vector3(1.85f, 0.88f, 0.84f));
            foreach (var renderer in sofa.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = olive;
            var cushion = FitPrefab(Doll + "16_Textiles/SM_LPI_Text_Cushion_01.prefab", rooms,
                "Cream sofa cushion", new Vector3(0.82f, 1.13f, -0.41f),
                new Vector3(0.38f, 0.31f, 0.18f));
            foreach (var renderer in cushion.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = cream;
            FitPrefab(Doll + "05_Tables/SM_LPI_Table_Coffee_02.prefab", rooms, "Low tea table",
                new Vector3(2.53f, 0.49f, -0.53f), new Vector3(0.82f, 0.42f, 0.53f));
            FitPrefab(Doll + "21_Fireplace/SM_LPI_Fire_Surround_01.prefab", rooms,
                "Living-room brick fireplace", new Vector3(3.32f, 1.24f, 0.49f),
                new Vector3(1.15f, 1.89f, 0.45f));
            FitPrefab(Doll + "21_Fireplace/SM_LPI_Fire_Logs_01.prefab", rooms,
                "Hearth fire and logs", new Vector3(3.32f, 0.49f, 0.23f),
                new Vector3(0.52f, 0.47f, 0.39f));
            var hearthLight = Child(rooms, "Fireplace glow");
            hearthLight.transform.localPosition = new Vector3(3.32f, 0.91f, -0.01f);
            var fireLight = hearthLight.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.47f, 0.18f);
            fireLight.intensity = 0.78f;
            fireLight.range = 2.2f;
            fireLight.shadows = LightShadows.None;
            FitPrefab(SmallProps + "P_Books_01.prefab", rooms, "Tea table books",
                new Vector3(2.53f, 0.75f, -0.49f), new Vector3(0.35f, 0.15f, 0.28f));
            FitPrefab(Doll + "15_Plants/SM_LPI_Plant_Succulent_01.prefab", rooms,
                "Living-room plant", new Vector3(3.04f, 0.49f, -0.31f),
                new Vector3(0.29f, 0.45f, 0.28f));

            // Upper left: bedroom, bedside table and small wall decor.
            FitPrefab(Doll + "07_Bedroom/SM_LPI_Bed_Double_01.prefab", rooms, "Bedroom bed",
                new Vector3(-2.93f, 3.30f, -0.02f), new Vector3(2.07f, 0.78f, 0.88f));
            var throwBlanket = FitPrefab(Doll + "16_Textiles/SM_LPI_Text_Throw_01.prefab", rooms,
                "Cream bed blanket", new Vector3(-2.89f, 3.68f, -0.20f),
                new Vector3(1.41f, 0.13f, 0.67f));
            foreach (var renderer in throwBlanket.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = cream;
            var pillow = FitPrefab(Doll + "16_Textiles/SM_LPI_Text_Cushion_02.prefab", rooms,
                "Bed pillow", new Vector3(-3.61f, 3.75f, -0.16f),
                new Vector3(0.41f, 0.19f, 0.27f));
            foreach (var renderer in pillow.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = cream;
            FitPrefab(Doll + "07_Bedroom/SM_LPI_Bed_Nightstand_01.prefab", rooms,
                "Bedroom nightstand", new Vector3(-1.31f, 3.20f, 0.29f),
                new Vector3(0.48f, 0.57f, 0.44f));
            FitPrefab(TavernProps + "Single Candle 2.prefab", rooms, "Bedside candle",
                new Vector3(-1.31f, 3.58f, 0.29f), new Vector3(0.15f, 0.25f, 0.15f));
            FitPrefab(SmallProps + "P_Clock.prefab", rooms, "Bedroom wall clock",
                new Vector3(-1.18f, 4.50f, 0.60f), new Vector3(0.36f, 0.36f, 0.15f));
            FitPrefab(Doll + "16_Textiles/SM_LPI_Text_RugRect_01.prefab", rooms,
                "Bedroom rug", new Vector3(-2.34f, 2.90f, -0.48f),
                new Vector3(1.30f, 0.04f, 0.53f));

            // Upper right: study and storage with warm timber tavern pieces.
            FitPrefab(TavernProps + "Dining Table.prefab", rooms, "Study timber table",
                new Vector3(1.37f, 3.26f, -0.22f), new Vector3(1.34f, 0.72f, 0.69f));
            FitPrefab(TavernProps + "Backing Chair.prefab", rooms, "Study chair",
                new Vector3(2.43f, 3.31f, -0.14f), new Vector3(0.57f, 0.82f, 0.51f));
            FitPrefab(Doll + "06_Storage/SM_LPI_Storage_Bookcase_03.prefab", rooms,
                "Study bookcase", new Vector3(3.48f, 3.89f, 0.41f),
                new Vector3(0.81f, 1.88f, 0.49f));
            FitPrefab(SmallProps + "P_Books_02.prefab", rooms, "Study books",
                new Vector3(1.22f, 3.73f, -0.19f), new Vector3(0.34f, 0.20f, 0.28f));
            FitPrefab(TavernProps + "Wooden Chest.prefab", rooms, "Study storage chest",
                new Vector3(0.68f, 2.99f, 0.37f), new Vector3(0.69f, 0.47f, 0.44f));
            FitPrefab(SmallProps + "P_Plant.prefab", rooms, "Study greenery",
                new Vector3(2.85f, 3.22f, 0.46f), new Vector3(0.35f, 0.51f, 0.34f));
        }

        private static Mesh GableMesh()
        {
            var path = LevelRoot + "/Art/L1_GablePanel.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
            var mesh = new Mesh { name = "L1 Gable Panel" };
            mesh.vertices = new[]
            {
                new Vector3(-4.13f, 5.43f, 0.72f),
                new Vector3(0f, 6.67f, 0.72f),
                new Vector3(4.13f, 5.43f, 0.72f)
            };
            mesh.uv = new[] { new Vector2(0, 0), new Vector2(.5f, 1), new Vector2(1, 0) };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static void CenterPrefab(GameObject instance, Vector3 center)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            instance.transform.position += center - bounds.center;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }

        private static Material Material(string name, Color color)
        {
            var path = LevelRoot + "/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                existing.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(existing);
                return existing;
            }
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader bulunamadı.");
            var material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.13f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ImportSprite(string path, bool alpha)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Resim bulunamadı: " + path);
            if (importer.textureType == TextureImporterType.Sprite && importer.alphaIsTransparency == alpha) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = alpha;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static GameObject Child(Transform parent, string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static void Sprite(GameObject parent, string name, string path, Vector3 position, float scale, int order)
        {
            var obj = Child(parent.transform, name);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * scale;
            var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            renderer.sortingOrder = order;
        }

        private static void Block(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void RoofBeam(Transform parent, string name, Vector3 position, float angle, float length, Material material)
        {
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = name;
            beam.transform.SetParent(parent, false);
            beam.transform.localPosition = position;
            beam.transform.localRotation = Quaternion.Euler(0, 0, angle);
            beam.transform.localScale = new Vector3(length, 0.23f, 1.65f);
            beam.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void SlimBrace(Transform parent, string name, Vector3 position, float angle,
            float length, Material material)
        {
            var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            beam.name = name;
            beam.transform.SetParent(parent, false);
            beam.transform.localPosition = position;
            beam.transform.localRotation = Quaternion.Euler(0, 0, angle);
            beam.transform.localScale = new Vector3(length, 0.12f, 0.18f);
            beam.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static GameObject FitPrefab(string path, Transform parent, string name, Vector3 center, Vector3 size)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("Prefab bulunamadı: " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("Prefab renderersız: " + path);
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            instance.transform.localScale = new Vector3(
                size.x / Mathf.Max(bounds.size.x, 0.001f),
                size.y / Mathf.Max(bounds.size.y, 0.001f),
                size.z / Mathf.Max(bounds.size.z, 0.001f));
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            instance.transform.position += parent.TransformPoint(center) - bounds.center;
            return instance;
        }
    }
}
