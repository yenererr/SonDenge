using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Jam.Levels.Level1
{
    internal static class L1_CozyPlayableBuilder
    {
        private const string RootPath = "Assets/Levels/Level1/";
        private const string Doll = "Assets/LowPolyInteriorsFree/Prefabs/";
        private const string Tavern = "Assets/Low Poly Tarvern Pack/Prefabs/Probs/";
        private const string Decor = "Assets/3Digitalis/LowPolyInteriorPropsPack/Prefabs/";
        private const string StylArts = "Assets/StylArts/StylizedHouseInterior/Level1/";
        private const string Farm = "Assets/FarmHouse/Models/SM_FarmHouse_";

        private static Material wood, lightWood, tealWood, plaster, iron, stone, olive, cream, glow, tile;
        private static Mesh beamMesh;
        private static Mesh salvagedBoards;
        private static Material salvagedWood;
        private static Material mixSofaMaterial, mixBookshelfMaterial;
        private static Material stylArtsTvMaterial, stylArtsFridgeMaterial, stylArtsBathMaterial, stylArtsStoveMaterial;
        private static Material stylArtsKitchenMaterial, stylArtsChairMaterial;
        private static Material farmMaterial;
        private static Material realisticBedMaterial;
        private static Material sewerLampMaterial, sewerLampGlassMaterial;
        private const string SewerLampPrefab = "Assets/Ata Khani/Modular Sewer Props/Prefabs/Lamp_Hanging_2M.prefab";
        private static int meshIndex;
        private static Transform house;
        private static readonly float[] Floors = { -1.15f, 1.10f, 3.35f };
        private static readonly List<Collider> FloorsPhysics = new List<Collider>();

        [MenuItem("Son Denge/Level1/Oynanabilir 3D Cozy Evi Kur")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("3D evi yeniden kurmadan önce Play modunu durdurun.");
                return;
            }
            if (EditorSceneManager.GetActiveScene().path != "Assets/Scenes/Level1.unity")
                throw new InvalidOperationException("Önce Level1 sahnesini açın.");

            Folder(RootPath + "Meshes");
            Folder(RootPath + "Materials");
            Folder(RootPath + "Art");

            // Textures & Materials
            EnsureCrackedPlasterTextures();
            plaster = Mat("L1_3D_AgedPlaster", new Color(0.96f, 0.93f, 0.88f));
            plaster.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(RootPath + "Art/L1_CrackedPlaster_BC.png"));
            var normTex = AssetDatabase.LoadAssetAtPath<Texture2D>(RootPath + "Art/L1_CrackedPlaster_NM.png");
            if (normTex != null)
            {
                plaster.SetTexture("_BumpMap", normTex);
                plaster.SetFloat("_BumpScale", 1.0f);
                plaster.EnableKeyword("_NORMALMAP");
            }
            plaster.SetFloat("_Smoothness", 0.08f);
            plaster.SetColor("_EmissionColor", new Color(0.12f, 0.09f, 0.06f));
            plaster.EnableKeyword("_EMISSION");
            EditorUtility.SetDirty(plaster);

            wood = Mat("L1_3D_WornOak", new Color(0.68f, 0.44f, 0.25f), true);
            lightWood = Mat("L1_3D_HoneyOak", new Color(0.92f, 0.72f, 0.45f), true);
            tealWood = Mat("L1_3D_TealWood", new Color(0.24f, 0.45f, 0.42f), true);
            iron = Mat("L1_3D_BlackIron", new Color(0.12f, 0.11f, 0.10f));
            iron.SetFloat("_Metallic", 0.65f);
            iron.SetFloat("_Smoothness", 0.35f);
            stone = Mat("L1_3D_OldStone", new Color(0.50f, 0.46f, 0.40f));
            olive = Mat("L1_3D_Olive", new Color(0.24f, 0.31f, 0.15f));
            cream = Mat("L1_3D_Linen", new Color(0.83f, 0.72f, 0.48f));
            tile = Mat("L1_3D_Clay", new Color(0.48f, 0.22f, 0.11f), true);

            glow = Mat("L1_3D_LanternGlow", new Color(1f, 0.69f, 0.25f));
            glow.EnableKeyword("_EMISSION");
            glow.SetColor("_EmissionColor", new Color(2.5f, 1.3f, 0.35f));
            EditorUtility.SetDirty(glow);

            // Furniture materials
            mixSofaMaterial = MixPbrMaterial("L1_MIX_BigSofa_URP", "Assets/ZNS3D/Mix_Furniture_Pack/Textures/Big_Sofa_1", "big_sofa_big_sofa");
            mixBookshelfMaterial = MixPbrMaterial("L1_MIX_Bookshelf_URP", "Assets/ZNS3D/Mix_Furniture_Pack/Textures/Bookshelf_1", "bookshelf_Mat_Bookshelf_1");
            stylArtsTvMaterial = StylArtsMaterial("L1_StylArts_TV_URP", new Color(0.39f, 0.23f, 0.15f));
            stylArtsFridgeMaterial = StylArtsMaterial("L1_StylArts_Fridge_URP", Color.white, "T_Fridge_SM_D.TGA", "T_Fridge_SM_N.TGA");
            stylArtsBathMaterial = StylArtsMaterial("L1_StylArts_Bathtub_URP", new Color(0.93f, 0.88f, 0.78f));
            stylArtsStoveMaterial = StylArtsMaterial("L1_StylArts_Stove_URP", Color.white, "T_Stove_A_D.TGA", "T_Stove_A_N.TGA");
            stylArtsKitchenMaterial = StylArtsMaterial("L1_StylArts_Kitchen_URP", Color.white, "T_Kitchen_Mat_D.TGA", "T_Kitchen_Mat_N.TGA");
            stylArtsChairMaterial = StylArtsMaterial("L1_StylArts_Chair_URP", Color.white, "T_LeatherCouch_BC.TGA", "T_LeatherCouch_N.TGA");
            realisticBedMaterial = RealisticBedMaterial();
            sewerLampMaterial = SewerLampMaterial();
            sewerLampGlassMaterial = SewerLampGlassMaterial();

            farmMaterial = FarmMaterial();
            beamMesh = CreateBeam();
            PrepareSalvagedBoards();
            meshIndex = 0;

            // Clear old house roots
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "[L1] World Visuals" || root.name.StartsWith("[L1] Asset Review"))
                    UnityEngine.Object.DestroyImmediate(root);
            }

            var world = new GameObject("[L1] World Visuals").transform;

            house = Node(world, "3D Ev — ahşap yapı ve bağımsız eşyalar");
            FloorsPhysics.Clear();

            // Build floors & foundation stilts
            for (var level = 0; level < 3; level++) BuildFloor(level);
            BuildRoof();
            BuildBalcony();
            BuildFoundationStilts();
            DressRooms();

            // The two functional gameplay climb routes
            var route1 = Ladder("Alt kattan orta kata", 1.55f, 0);
            var route2 = Ladder("Orta kattan çatı katına", -0.85f, 1);
            CreateResident(new[] { route1, route2 });

            house.localPosition = new Vector3(0, -0.45f, 0);
            var resident = house.GetComponentInChildren<L1_ResidentController>();
            if (resident != null) resident.spawnPosition = resident.transform.position;

            // Camera setup: Solid studio black contrast
            var camera = Root("Main Camera").GetComponent<Camera>();
            camera.transform.SetPositionAndRotation(new Vector3(0, 2.75f, -14.0f), Quaternion.Euler(3.5f, 0, 0));
            camera.orthographic = false;
            camera.fieldOfView = 41;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.015f, 0.02f, 1f);

            var controls = Node(world, "Eşya taşıma").gameObject.AddComponent<L1_PropDrag>();
            controls.sceneCamera = camera;
            controls.houseRoot = house;

            // Warm key sunlight coming softly from top-right / front
            var sun = Root("Directional Light").GetComponent<Light>();
            sun.transform.rotation = Quaternion.Euler(26f, -18f, 0f);
            sun.color = new Color(1f, 0.92f, 0.80f);
            sun.intensity = 0.70f;
            sun.shadows = LightShadows.Soft;
            sun.shadowNormalBias = 0.4f;

            // Dramatic contrast ambient lighting
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.12f, 0.10f, 0.15f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.12f, 0.09f);
            RenderSettings.ambientGroundColor = new Color(0.05f, 0.04f, 0.03f);

            L1_ProjectFilesInstaller.Apply(false);
            L1_CloudSeaBuilder.BuildInto(world);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            Debug.Log("Level1 3D Ev hazırlandı: eşyalar geri yerleştirildi, gereksiz merdivenler kaldırıldı, taş kaideler, zemin kazıkları, sıva dokusu ve sıcak fener aydınlatması korundu.");
        }

        [MenuItem("Son Denge/Level1/Oynanabilir 3D Cozy Evi Kur", true)]
        private static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        private static void BuildFloor(int level)
        {
            float y = Floors[level];
            var floor = Node(house, "Kat " + (level + 1));

            // Walking surface
            var physics = Node(floor, "Yürünebilir zemin");
            physics.localPosition = new Vector3(0, y - 0.10f, 0);
            var box = physics.gameObject.AddComponent<BoxCollider>();
            box.size = new Vector3(6.65f, 0.20f, 2.6f);
            FloorsPhysics.Add(box);

            // Weathered floorboards
            for (int i = 0; i < 22; i++)
            {
                float x = -3.23f + i * 0.307f;
                float hatchX = level == 1 ? 1.55f : -0.85f;
                float front = level > 0 && Mathf.Abs(x - hatchX) < 0.44f ? -0.25f : -1.15f;
                Beam(floor, "Aşınmış döşeme tahtası", new Vector3(x, y - 0.07f, front), new Vector3(x + 0.015f * Mathf.Sin(i), y - 0.07f, 1.2f), 0.13f, 0.29f, i % 3 == 0 ? wood : lightWood);
            }

            FarmPart(floor, "FarmHouse — sol ahşap döşeme", "Terrace_4M_001", new Vector3(-1.7f, y - 0.055f, 0.03f), new Vector3(3.3f, 0.12f, 2.47f));
            FarmPart(floor, "FarmHouse — sağ ahşap döşeme", "Terrace_4M_001", new Vector3(1.7f, y - 0.055f, 0.03f), new Vector3(3.3f, 0.12f, 2.47f));

            // Heavy floor beams
            Beam(floor, "Kalın ön döşeme kirişi", new Vector3(-3.5f, y - 0.17f, -1.24f), new Vector3(3.5f, y - 0.13f, -1.24f), 0.22f, 0.30f, wood);
            Beam(floor, "Arka döşeme kirişi", new Vector3(-3.4f, y - 0.15f, 1.2f), new Vector3(3.4f, y - 0.15f, 1.2f), 0.23f, 0.26f, wood);
            SalvagedPatch(floor, new Vector3(-2.1f, y + 0.004f, 0.50f), new Vector3(2.1f, 0.045f, 1.30f));
            SalvagedPatch(floor, new Vector3(0.2f, y + 0.004f, 0.50f), new Vector3(2.0f, 0.045f, 1.30f));

            // Pegged beam ends with iron studs
            for (int i = 0; i < 8; i++)
            {
                float x = -3.25f + i * 0.93f;
                Beam(floor, "Zemin altı kiriş ucu", new Vector3(x, y - 0.3f, -1.4f), new Vector3(x, y - 0.3f, 1.25f), 0.17f, 0.19f, wood);
                Stud(floor, new Vector3(x, y - 0.15f, -1.405f));
            }

            // Back wall with cracked plaster and exposed brick/masonry
            var back = Node(floor, "Eskimiş arka duvar");
            float windowX = level == 1 ? -2.25f : -2.20f;
            WallPanel(back, -3.3f, windowX - 0.52f, y, y + 2.2f);
            WallPanel(back, windowX + 0.52f, 3.3f, y, y + 2.2f);
            WallPanel(back, windowX - 0.52f, windowX + 0.52f, y, y + 0.58f);

            var archTop = new List<Vector3>();
            for (int i = 0; i <= 16; i++)
            {
                float x = windowX - 0.52f + i * 1.04f / 16;
                float arch = y + 1.46f + Mathf.Sqrt(Mathf.Max(0, 0.52f * 0.52f - (x - windowX) * (x - windowX)));
                archTop.Add(new Vector3(x, arch, 1.18f));
                archTop.Add(new Vector3(x, y + 2.2f, 1.18f));
            }
            Strip(back, "Duvar kemeri", archTop, plaster);
            Arch(back, windowX, y + 0.58f, 0.52f, 0.88f, wood);
            Beam(back, "Pencere dikmesi", new Vector3(windowX, y + 0.58f, 1.03f), new Vector3(windowX, y + 1.97f, 1.03f), 0.045f, 0.065f, lightWood);
            Beam(back, "Pencere çaprazı", new Vector3(windowX - 0.5f, y + 1.18f, 1.03f), new Vector3(windowX + 0.5f, y + 1.18f, 1.03f), 0.04f, 0.065f, lightWood);
            Beam(back, "Pencere pervazı", new Vector3(windowX - 0.65f, y + 0.55f, 0.99f), new Vector3(windowX + 0.65f, y + 0.55f, 0.99f), 0.12f, 0.28f, wood);

            // Back wall decorative details: Stone skirt and window arch
            FarmPart(back, "FarmHouse — kalın ahşap pencere kemeri", "WoodenArch_2m_001", new Vector3(windowX, y + 1.31f, 0.92f), new Vector3(1.12f, 1.55f, 0.25f));
            FarmPart(back, "FarmHouse — aşınmış taş duvar eteği", "StoneBlocks_Front_2m_66_7cm_001", new Vector3(level == 1 ? 0.45f : -0.25f, y + 0.15f, 0.815f), new Vector3(1.52f, 0.28f, 0.10f));

            // Protruding 3D single bricks along plaster edges
            for (int b = 0; b < 4; b++)
            {
                float bx = -1.2f + b * 0.85f + (level * 0.2f);
                float by = y + 0.22f + (b % 2) * 0.28f;
                FarmPart(back, "Dökülen sıva tuğlası " + b, (b % 2 == 0) ? "SingleBrick_A_001" : "SingleBrick_C_001", new Vector3(bx, by, 0.95f), new Vector3(0.24f, 0.12f, 0.14f));
            }

            // Columns, Stone Plinths (Taş Kaideler) & Corner Rivets
            for (int s = -1; s <= 1; s += 2)
            {
                float x = s * 3.26f;

                // Stone Plinth (Taş Kaide) at the base of the column
                StonePlinth(floor, new Vector3(x, y + 0.16f, -0.95f));

                // Chunky rustic wood beam column
                Beam(floor, "Eğri ana ahşap direk", new Vector3(x, y - 0.25f, -0.95f), new Vector3(x + s * 0.07f, y + 2.20f, -0.91f), 0.24f, 0.27f, wood);
                FarmTimber(floor, "FarmHouse — eğri ön taşıyıcı", "WoodBeam_Dark_4m_A_001", new Vector3(x, y - 0.25f, -1.03f), new Vector3(x + s * 0.07f, y + 2.20f, -1.00f), 0.30f);

                Beam(floor, "Arka ana direk", new Vector3(x, y, 1.08f), new Vector3(x + s * 0.05f, y + 2.22f, 1.08f), 0.20f, 0.24f, wood);
                FarmTimber(floor, "FarmHouse — arka direk", "WoodBeam_Bright_4m_B_001", new Vector3(x, y, 1.00f), new Vector3(x + s * 0.05f, y + 2.22f, 1.00f), 0.22f);

                Beam(floor, "Çapraz ahşap payanda", new Vector3(x, y + 0.2f, -0.92f), new Vector3(x - s * 0.51f, y + 0.84f, -0.92f), 0.14f, 0.16f, lightWood);
                FarmTimber(floor, "FarmHouse — çapraz payanda", "WallBeam_2m_001", new Vector3(x, y + 0.2f, -1.035f), new Vector3(x - s * 0.51f, y + 0.84f, -1.035f), 0.16f);
                Beam(floor, "Tavan köşe desteği", new Vector3(x, y + 1.62f, -0.92f), new Vector3(x - s * 0.58f, y + 2.15f, -0.92f), 0.14f, 0.16f, wood);

                // Forged iron corner strap plate & rivets
                CornerStrap(floor, new Vector3(x, y - 0.12f, -1.26f), s);

                var wall = Node(floor, "Yan duvar çarpışması");
                wall.localPosition = new Vector3(s * 3.47f, y + 1.1f, 0.1f);
                var collider = wall.gameObject.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.16f, 2.2f, 2.6f);

                for (int plank = 0; plank < 5; plank++)
                    Beam(floor, "Yan duvar eski ahşap kaplama", new Vector3(x, y + 0.05f, 0.25f + plank * 0.20f), new Vector3(x + s * 0.04f, y + 2.15f, 0.25f + plank * 0.20f), 0.19f, 0.12f, plank % 2 == 0 ? wood : lightWood);
            }

            // Horizontal timber trim across the wall
            Beam(back, "Yatay duvar ahşabı", new Vector3(-1.55f, y + 1.25f, 1.03f), new Vector3(3.28f, y + 1.29f, 1.03f), 0.095f, 0.12f, wood);

            // Intermediate column with stone base (near ladder / middle)
            if (level < 2)
            {
                float midX = (level == 0) ? -1.05f : -0.85f;
                StonePlinth(floor, new Vector3(midX, y + 0.16f, -0.95f));
                Beam(floor, "İç ahşap kolon", new Vector3(midX, y, -0.95f), new Vector3(midX, y + 2.20f, -0.95f), 0.20f, 0.22f, wood);
            }

            // Door on the right of each floor
            if (level == 1)
            {
                Door(back, 2.50f, y + 0.03f, 0.46f, 1.15f, tealWood, true);
            }
            else
            {
                Door(back, 2.50f, y + 0.03f, 0.42f, 1.10f, wood, level == 2);
            }

            // Ambient hanging lamps (Modular Sewer Props)
            for (int l = 0; l < 2; l++)
                Lamp(floor, new Vector3(l == 0 ? -0.85f : 1.55f, y + 2.22f, 0.35f));

            if (level != 1)
            {
                Shelf(floor, new Vector3(-0.25f, y + 1.30f, 0.86f), 1.20f);
                Shelf(floor, new Vector3(-0.15f, y + 1.68f, 0.86f), 1.05f);
                Prop(Decor + "P_Plant.prefab", floor, "Raf üzerindeki sarmaşık", new Vector3(0.65f, y + 1.91f, 0.80f), new Vector3(0.32f, 0.40f, 0.26f));
            }
        }

        private static void BuildFoundationStilts()
        {
            var stiltGroup = Node(house, "Zemin Kazıkları ve İskele Tabanı");
            float yBase = Floors[0] - 0.18f;

            // Heavy timber bottom apron beams running along the deck base
            Beam(stiltGroup, "Ön taban ana kirişi", new Vector3(-3.65f, yBase - 0.14f, -1.25f), new Vector3(3.65f, yBase - 0.14f, -1.25f), 0.26f, 0.35f, wood);
            Beam(stiltGroup, "Orta taban kirişi", new Vector3(-3.5f, yBase - 0.16f, 0f), new Vector3(3.5f, yBase - 0.16f, 0f), 0.24f, 0.30f, wood);
            Beam(stiltGroup, "Arka taban kirişi", new Vector3(-3.5f, yBase - 0.16f, 1.2f), new Vector3(3.5f, yBase - 0.16f, 1.2f), 0.24f, 0.30f, wood);

            // Vertical wooden pilings / stilts (kazıklar) driving down
            float[] stiltX = { -3.45f, -2.85f, -2.15f, -1.45f, -0.75f, 0.0f, 0.75f, 1.45f, 2.15f, 2.85f, 3.45f };
            for (int i = 0; i < stiltX.Length; i++)
            {
                float x = stiltX[i];
                float depthDrop = (i == 0 || i == stiltX.Length - 1) ? 1.45f : 0.95f + (i % 3) * 0.20f;
                float bottomY = yBase - depthDrop;

                // Front vertical stilt
                Beam(stiltGroup, "Taban kazığı " + i, new Vector3(x, yBase - 0.14f, -1.25f), new Vector3(x, bottomY, -1.25f), 0.22f, 0.24f, wood);
                FarmTimber(stiltGroup, "FarmHouse — kazık gövdesi " + i, "WoodBeam_Dark_4m_C_001", new Vector3(x, yBase - 0.14f, -1.25f), new Vector3(x, bottomY, -1.25f), 0.25f);

                // Sub-floor joist beam going from front to back
                Beam(stiltGroup, "Taban enine kiriş " + i, new Vector3(x, yBase - 0.12f, -1.25f), new Vector3(x, yBase - 0.12f, 1.25f), 0.16f, 0.20f, lightWood);

                // Iron collar bolts on pilings
                Stud(stiltGroup, new Vector3(x, yBase - 0.10f, -1.38f));
                Stud(stiltGroup, new Vector3(x, yBase - 0.28f, -1.38f));
            }

            // Heavy diagonal knee braces on outer stilts
            Beam(stiltGroup, "Sol kazık çapraz payanda", new Vector3(-3.45f, yBase - 0.85f, -1.22f), new Vector3(-2.65f, yBase - 0.14f, -1.22f), 0.16f, 0.18f, wood);
            Beam(stiltGroup, "Sağ kazık çapraz payanda", new Vector3(3.45f, yBase - 0.85f, -1.22f), new Vector3(2.65f, yBase - 0.14f, -1.22f), 0.16f, 0.18f, wood);

            // Corner heavy stone footing blocks underneath the extreme corner stilts
            FarmPart(stiltGroup, "Sol taban taş kaidesi", "SingleStoneBlock_A_001", new Vector3(-3.45f, yBase - 1.55f, -1.25f), new Vector3(0.55f, 0.40f, 0.55f), false);
            FarmPart(stiltGroup, "Sağ taban taş kaidesi", "SingleStoneBlock_B_001", new Vector3(3.45f, yBase - 1.55f, -1.25f), new Vector3(0.55f, 0.40f, 0.55f), false);
        }

        private static void StonePlinth(Transform parent, Vector3 pos)
        {
            var plinth = Node(parent, "Taş Kaide");
            plinth.localPosition = pos;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Kaide Taşı";
            cube.transform.SetParent(plinth, false);
            cube.transform.localScale = new Vector3(0.36f, 0.30f, 0.36f);
            UnityEngine.Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.GetComponent<Renderer>().sharedMaterial = stone;

            var collar = Node(plinth, "Demir Kuşak");
            collar.localPosition = new Vector3(0, 0.18f, 0);
            var collarCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            collarCube.transform.SetParent(collar, false);
            collarCube.transform.localScale = new Vector3(0.28f, 0.08f, 0.28f);
            UnityEngine.Object.DestroyImmediate(collarCube.GetComponent<Collider>());
            collarCube.GetComponent<Renderer>().sharedMaterial = iron;

            Stud(collar, new Vector3(0, 0, -0.15f));
            Stud(collar, new Vector3(0.15f, 0, 0));
            Stud(collar, new Vector3(-0.15f, 0, 0));
        }

        private static void CornerStrap(Transform parent, Vector3 pos, int side)
        {
            var strap = Node(parent, "Köşe Dövme Demir Köşebent");
            strap.localPosition = pos;
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Demir Plaka";
            plate.transform.SetParent(strap, false);
            plate.transform.localScale = new Vector3(0.28f, 0.22f, 0.035f);
            UnityEngine.Object.DestroyImmediate(plate.GetComponent<Collider>());
            plate.GetComponent<Renderer>().sharedMaterial = iron;

            Stud(strap, new Vector3(-0.09f, -0.06f, -0.025f));
            Stud(strap, new Vector3(0.09f, -0.06f, -0.025f));
            Stud(strap, new Vector3(-0.09f, 0.06f, -0.025f));
            Stud(strap, new Vector3(0.09f, 0.06f, -0.025f));
        }

        private static void BuildRoof()
        {
            var roof = Node(house, "Asimetrik kiremit çatı");
            for (int row = 0; row < 8; row++) for (int i = 0; i < 19; i++)
            {
                float x = -3.56f + i * 0.40f + (row % 2) * 0.13f;
                float y = 5.36f + (x + 3.58f) * 0.145f;
                FarmPart(roof, "FarmHouse — ayrı kiremit " + row + " / " + i,
                    (i + row) % 3 == 0 ? "Roof_Top_Shingle_A_001" : (i + row) % 3 == 1 ? "Roof_Top_Shingle_B_001" : "Roof_Top_Shingle_C_001",
                    new Vector3(x, y + 0.03f + row * 0.065f, -1.48f + row * 0.37f), new Vector3(0.42f, 0.19f, 0.39f));
            }

            Beam(roof, "Eğimli ön çatı saçağı", new Vector3(-3.8f, 5.36f, -1.45f), new Vector3(3.8f, 6.46f, -1.45f), 0.23f, 0.30f, wood);
            for (int i = 0; i < 2; i++)
                FarmTimber(roof, "FarmHouse — eğri mahya kirişi", "TopRoofBeam_4m_A_001",
                    new Vector3(-3.75f + i * 3.75f, 5.34f + i * 0.55f, -1.51f),
                    new Vector3(0f + i * 3.75f, 5.89f + i * 0.55f, -1.51f), 0.28f);

            Beam(roof, "İç çatı kirişi", new Vector3(-3.7f, 5.23f, -1.01f), new Vector3(3.7f, 6.30f, -1.01f), 0.14f, 0.18f, lightWood);
            for (int i = 0; i < 10; i++)
            {
                float x = -3.4f + i * 0.75f;
                float y = 5.4f + (x + 3.58f) * 0.145f;
                Beam(roof, "Çatı merteği", new Vector3(x, y - 0.10f, -1.52f), new Vector3(x, y + 0.07f, 1.3f), 0.12f, 0.16f, lightWood);
            }

            var verts = new List<Vector3> { new Vector3(-3.3f, 5.40f, 1.18f), new Vector3(3.3f, 5.40f, 1.18f), new Vector3(3.3f, 6.55f, 1.18f), new Vector3(-3.3f, 5.61f, 1.18f) };
            Polygon(roof, "Üst çatı sıvası", verts, plaster);
            Beam(roof, "Çatıya uzanan sağ direk", new Vector3(3.33f, 5.35f, -0.91f), new Vector3(3.35f, 6.28f, -0.91f), 0.22f, 0.25f, wood);
            Beam(roof, "Üst çatı duvar çaprazı", new Vector3(-0.1f, 5.55f, 1.04f), new Vector3(2.5f, 6.22f, 1.04f), 0.10f, 0.12f, wood);

            // Rustic stone chimney on the left
            FarmPart(roof, "FarmHouse — taş baca", "Chimney_001", new Vector3(-2.6f, 6.19f, 0.70f), new Vector3(0.55f, 1.35f, 0.55f), false);

            // Keep interior back wall brightly and cleanly illuminated without roof blotches
            foreach (var r in roof.GetComponentsInChildren<MeshRenderer>())
                r.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static void BuildBalcony()
        {
            var balcony = Node(house, "Ahşap balkon ve vinç");
            for (int level = 1; level < 3; level++)
            {
                float y = Floors[level];
                Beam(balcony, "Balkon döşemesi", new Vector3(3.3f, y - 0.12f, -0.6f), new Vector3(4.1f, y - 0.12f, -0.6f), 0.16f, 1.6f, lightWood);
                for (int i = 0; i < 4; i++)
                    Beam(balcony, "Balkon korkuluğu", new Vector3(3.35f + i * 0.23f, y, -1.28f), new Vector3(3.35f + i * 0.23f, y + 0.68f, -1.28f), 0.055f, 0.07f, wood);
                Beam(balcony, "Balkon küpeştesi", new Vector3(3.28f, y + 0.7f, -1.28f), new Vector3(4.12f, y + 0.74f, -1.28f), 0.08f, 0.10f, wood);

                Beam(balcony, "Balkon payandası", new Vector3(3.25f, y - 0.8f, -0.4f), new Vector3(4, y - 0.12f, -0.4f), 0.14f, 0.16f, wood);
                FarmTimber(balcony, "FarmHouse — balkon çaprazı", "WoodBeam_Bright_4m_B_001", new Vector3(3.25f, y - 0.72f, -0.55f), new Vector3(4.12f, y - 0.10f, -0.55f), 0.16f);
            }

            // Upper Crane & hanging crate
            Beam(balcony, "Vinç kolu", new Vector3(3.12f, 5.45f, 0.2f), new Vector3(4.65f, 5.75f, 0.2f), 0.18f, 0.22f, wood);
            Beam(balcony, "Vinç çaprazı", new Vector3(3.25f, 4.6f, 0.2f), new Vector3(4.45f, 5.70f, 0.2f), 0.14f, 0.16f, wood);
            FarmTimber(balcony, "FarmHouse — eğri vinç kolu", "WoodBeam_Dark_4m_D_001", new Vector3(3.12f, 5.45f, 0.08f), new Vector3(4.78f, 5.77f, 0.08f), 0.24f);
            FarmTimber(balcony, "FarmHouse — vinç desteği", "WoodBeam_Bright_4m_A_001", new Vector3(3.25f, 4.62f, 0.08f), new Vector3(4.63f, 5.72f, 0.08f), 0.17f);

            // Pulley wheel, rope & hanging crate
            Ellipsoid(balcony, "Vinç makarası", new Vector3(4.72f, 5.77f, 0.08f), new Vector3(0.18f, 0.18f, 0.06f), iron);
            Beam(balcony, "Vinç halatı", new Vector3(4.72f, 5.77f, 0.08f), new Vector3(4.72f, 4.45f, 0.08f), 0.026f, 0.026f, cream);
            FarmPart(balcony, "FarmHouse — asılı kargo sandığı", "WoodenCrate_001", new Vector3(4.72f, 4.15f, 0.08f), new Vector3(0.68f, 0.58f, 0.65f));
        }

        private static void DressRooms()
        {
            var p = Node(house, "Bağımsız 3D mobilyalar");

            // ==========================================
            // KAT 1 MOBİLYALARI
            // ==========================================
            var bathtub = Movable(StylArts + "Meshes/SM_Bathtub.fbx", p, "StylArts — küvet", -2.35f, 0, new Vector3(1.20f, 0.68f, 0.85f), 5, stylArtsBathMaterial);
            bathtub.localRotation = Quaternion.Euler(0, 180, 0);

            Movable(StylArts + "Meshes/SM_Bathroom_Sink.fbx", p, "StylArts — banyo lavabosu ve musluğu", -1.25f, 0, new Vector3(0.56f, 0.77f, 0.45f), 3, stylArtsBathMaterial);
            Prop(Doll + "12_Bathroom/SM_LPI_Bath_Mirror_01.prefab", p, "Banyo aynası", new Vector3(-1.5f, Floors[0] + 1.46f, 0.96f), new Vector3(0.48f, 0.62f, 0.07f));

            // Ocak ve fırın (Düzgün orantılı, kameraya dönük, yanında lavabo yok)
            var stove = Movable(StylArts + "Meshes/SM_Stove_A.fbx", p, "StylArts — ocak ve fırın", 0f, 0, new Vector3(0.85f, 0.90f, 0.85f), 4, stylArtsStoveMaterial, Quaternion.Euler(0, 180, 0));
            Prop(Doll + "10_Kitchenware/SM_LPI_Ware_Kettle_01.prefab", stove, "Ocaktaki çaydanlık", new Vector3(0.10f, 0.48f, 0.05f), new Vector3(0.20f, 0.22f, 0.20f));

            // Buzdolabı (Düzgün orantılı, merdivene çarpmayan, kapak kameraya dönük)
            var fridge = Movable(StylArts + "Meshes/SM_Fridge.fbx", p, "StylArts — buzdolabı", 1.35f, 0, new Vector3(0.82f, 1.65f, 0.82f), 5, stylArtsFridgeMaterial, Quaternion.identity);

            // ==========================================
            // KAT 2 MOBİLYALARI
            // ==========================================
            var mixSofa = Movable("Assets/ZNS3D/Mix_Furniture_Pack/Meshes/big_sofa.fbx", p, "MIX — yıpranmış deri kanepe", -1.50f, 1, new Vector3(1.75f, 0.9f, 0.82f), 5, mixSofaMaterial);
            mixSofa.localRotation = Quaternion.Euler(0, 180, 0);

            Movable(StylArts + "Meshes/SM_LeatherCouch.fbx", p, "StylArts — orta kat tekli koltuk", 2.62f, 1, new Vector3(0.92f, 0.82f, 0.72f), 2, stylArtsChairMaterial);
            Movable("Assets/ZNS3D/Mix_Furniture_Pack/Meshes/tea_table.fbx", p, "MIX — sehpa", -0.08f, 1, new Vector3(0.60f, 0.46f, 0.54f), 2, lightWood);
            Movable(StylArts + "Meshes/SM_TVSet.fbx", p, "StylArts — eski televizyon", 0.86f, 1, new Vector3(1.03f, 0.80f, 0.53f), 3, stylArtsTvMaterial);

            // ==========================================
            // KAT 3 MOBİLYALARI
            // ==========================================
            // Yatak: Başlık sol duvara dayalı, boyutu ve oranları bozulmadan düzgün
            var bed = Movable("Assets/Realistic_Bed/Prefab/Bed_Prefab.prefab", p, "Realistic Bed 01 — yatak", -2.05f, 2, new Vector3(1.60f, 1.00f, 2.15f), 4, realisticBedMaterial, Quaternion.Euler(0, 90, 0));
            Movable(Doll + "07_Bedroom/SM_LPI_Bed_Nightstand_01.prefab", p, "Komodin", -0.65f, 2, new Vector3(0.5f, 0.57f, 0.45f), 1);

            var bookcase = Movable("Assets/ZNS3D/Mix_Furniture_Pack/Meshes/bookshelf.fbx", p, "MIX — dokulu kitaplık", 1.3f, 2, new Vector3(0.95f, 1.48f, 0.48f), 4, mixBookshelfMaterial);
            bookcase.localRotation = Quaternion.Euler(0, 180, 0);
            for (int shelf = 0; shelf < 3; shelf++)
                Prop(Decor + "P_Books_02.prefab", bookcase, "Kitaplar", new Vector3(-0.1f, 0.10f + shelf * 0.25f, -0.10f), new Vector3(0.5f, 0.18f, 0.18f));

            Movable(Tavern + "Wooden Chest.prefab", p, "Çeyiz sandığı", 2.65f, 2, new Vector3(0.7f, 0.5f, 0.48f), 2);

            // Bitkiler ve kilimler
            for (int level = 0; level < 3; level++)
            {
                Prop(Decor + "P_Plant.prefab", p, "Saksı bitkisi", new Vector3(3.0f, Floors[level] + 0.25f, -0.05f), new Vector3(0.30f, 0.50f, 0.30f));
                Prop(Doll + "16_Textiles/SM_LPI_Text_RugRect_01.prefab", p, "Dokuma kilim", new Vector3(-1.5f, Floors[level] + 0.018f, -0.15f), new Vector3(1.9f, 0.025f, 1.2f), level == 0 ? cream : olive);
            }
        }

        private static Transform Movable(string path, Transform parent, string name, float x, int floor, Vector3 size, float mass, Material material = null, Quaternion? rootRotation = null)
        {
            var root = Node(parent, name);
            root.localPosition = new Vector3(x, Floors[floor] + size.y * 0.5f + 0.03f, 0.25f);
            if (rootRotation.HasValue) root.localRotation = rootRotation.Value;
            Prop(path, root, "Model", Vector3.zero, size, material, null);
            var box = root.gameObject.AddComponent<BoxCollider>();
            box.size = size;
            var body = root.gameObject.AddComponent<Rigidbody>();
            body.mass = mass;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.linearDamping = 0.7f;
            body.angularDamping = 2f;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            var movable = root.gameObject.AddComponent<L1_MovableProp>();
            movable.displayName = name;
            movable.itemId = "l1-" + name;
            return root;
        }

        private static L1_ClimbRoute Ladder(string name, float x, int level)
        {
            float low = Floors[level], high = Floors[level + 1];
            var root = Node(house, name);
            FarmPart(root, "FarmHouse — gerçek ahşap merdiven", "Ladder_2m_001", new Vector3(x, (low + high) * 0.5f, -0.66f), new Vector3(0.62f, 2.4f, 0.17f));
            var route = root.gameObject.AddComponent<L1_ClimbRoute>();
            route.bottom = Node(root, "Alt giriş");
            route.bottom.position = new Vector3(x, low + 0.43f, -0.96f);
            route.top = Node(root, "Üst giriş");
            route.top.position = new Vector3(x, high + 0.43f, -0.96f);
            route.landing = Node(root, "Üst çıkış");
            route.landing.position = new Vector3(x + 0.65f, high + 0.43f, -0.96f);
            route.upperFloor = FloorsPhysics[level + 1];
            route.hatch = Node(root, "Açılır merdiven kapağı");
            route.hatch.position = new Vector3(x, high + 0.025f, -0.66f);
            Beam(route.hatch, "Kapak tahtası", new Vector3(-0.34f, 0, 0), new Vector3(0.34f, 0, 0), 0.06f, 0.7f, wood);
            return route;
        }

        private static void CreateResident(L1_ClimbRoute[] routes)
        {
            var root = Node(house, "Oyuncu — evin sakini");
            root.position = new Vector3(-1.45f, Floors[0] + 0.43f, -0.96f);
            root.tag = "Player";
            var capsule = root.gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 0.84f;
            capsule.radius = 0.18f;
            var body = root.gameObject.AddComponent<Rigidbody>();
            body.mass = 1;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var visual = Node(root, "3D sakin görünümü");
            Ellipsoid(visual, "Kaban", new Vector3(0, -0.08f, 0), new Vector3(0.38f, 0.49f, 0.29f), olive);
            Ellipsoid(visual, "Yüz", new Vector3(0, 0.23f, -0.03f), new Vector3(0.29f, 0.27f, 0.25f), cream);
            Ellipsoid(visual, "Şapka kenarı", new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.065f, 0.34f), olive);
            Beam(visual, "Sivri ahşap renkli şapka", new Vector3(0, 0.35f, 0), new Vector3(-0.10f, 0.65f, 0.01f), 0.17f, 0.21f, olive);
            Ellipsoid(visual, "Burun", new Vector3(0, 0.23f, -0.17f), new Vector3(0.09f, 0.08f, 0.10f), cream);
            Ellipsoid(visual, "Sol göz", new Vector3(-0.065f, 0.27f, -0.145f), Vector3.one * 0.035f, iron);
            Ellipsoid(visual, "Sağ göz", new Vector3(0.065f, 0.27f, -0.145f), Vector3.one * 0.035f, iron);
            var left = Ellipsoid(visual, "Sol çizme", new Vector3(-0.105f, -0.35f, -0.035f), new Vector3(0.14f, 0.18f, 0.22f), wood);
            var right = Ellipsoid(visual, "Sağ çizme", new Vector3(0.105f, -0.35f, -0.035f), new Vector3(0.14f, 0.18f, 0.22f), wood);
            Ellipsoid(visual, "Sol kol", new Vector3(-0.23f, -0.06f, 0), new Vector3(0.12f, 0.32f, 0.13f), olive);
            Ellipsoid(visual, "Sağ kol", new Vector3(0.23f, -0.06f, 0), new Vector3(0.12f, 0.32f, 0.13f), olive);
            var player = root.gameObject.AddComponent<L1_ResidentController>();
            player.ladders = routes;
            player.visual = visual;
            player.leftFoot = left;
            player.rightFoot = right;
            player.spawnPosition = root.position;
        }

        private static void Shelf(Transform parent, Vector3 pos, float width)
        {
            Prop(Decor + "P_Shelf.prefab", parent, "Ahşap duvar rafı", pos, new Vector3(width, 0.09f, 0.28f), wood);
            for (int i = 0; i < 4; i++)
                Prop(Tavern + (i % 2 == 0 ? "Fat Mead Jug.prefab" : "Thin Wine Bottle 1.prefab"), parent, "Rafta seramik ve şişeler", pos + new Vector3(-width * 0.35f + i * width * 0.23f, 0.15f, -0.02f), new Vector3(0.14f, 0.24f, 0.14f));
        }

        private static void Lamp(Transform parent, Vector3 pos)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SewerLampPrefab);
            if (source == null) return;
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = "Modular Sewer — Sarkan Lamba";
            instance.transform.localPosition = pos;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one * 0.45f;

            foreach (var col in instance.GetComponentsInChildren<Collider>())
                UnityEngine.Object.DestroyImmediate(col);

            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                if (mats.Length > 0) mats[0] = sewerLampMaterial;
                if (mats.Length > 1) mats[1] = sewerLampGlassMaterial;
                r.sharedMaterials = mats;
            }

            var lodGroup = instance.GetComponent<LODGroup>();
            if (lodGroup != null)
            {
                var lods = lodGroup.GetLODs();
                for (int i = 0; i < lods.Length; i++)
                    lods[i].screenRelativeTransitionHeight = 0.005f * (lods.Length - i);
                lodGroup.SetLODs(lods);
            }

            // Warm amber (#FFB040) Point Light right inside the lamp cage
            var lightObj = Node(instance.transform, "Kehribar Lamba Işığı");
            lightObj.localPosition = new Vector3(0, -2.10f, 0);
            var light = lightObj.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1.0f, 0.69f, 0.25f); // #FFB040
            light.intensity = 2.8f;
            light.range = 4.8f;
            light.shadows = LightShadows.None;
        }

        private static void Door(Transform parent, float x, float y, float radius = 0.38f, float stem = 1.05f, Material doorMat = null, bool doubleDoor = false)
        {
            if (doorMat == null) doorMat = lightWood;
            var root = Node(parent, doubleDoor ? "Kemerli Çift Kanatlı Ahşap Kapı" : "Kemerli Ahşap Kapı");
            root.localPosition = Vector3.zero;

            Arch(root, x, y, radius, stem, wood);

            int plankCount = doubleDoor ? 8 : 6;
            float totalW = radius * 1.85f;
            for (int i = 0; i < plankCount; i++)
            {
                float dx = -totalW * 0.5f + i * (totalW / (plankCount - 1));
                float h = stem + Mathf.Sqrt(Mathf.Max(0, radius * radius - dx * dx));
                Beam(root, "Kapı Tahtası " + i, new Vector3(x + dx, y, 1.01f), new Vector3(x + dx, y + h, 1.01f), totalW / plankCount * 0.95f, 0.08f, doorMat);
            }

            // Heavy forged iron strap hinges
            Beam(root, "Alt Kapı Demiri", new Vector3(x - radius * 0.9f, y + 0.35f, 0.94f), new Vector3(x + radius * 0.9f, y + 0.35f, 0.94f), 0.05f, 0.035f, iron);
            Beam(root, "Üst Kapı Demiri", new Vector3(x - radius * 0.9f, y + 0.95f, 0.94f), new Vector3(x + radius * 0.9f, y + 0.95f, 0.94f), 0.05f, 0.035f, iron);
            Stud(root, new Vector3(x - radius * 0.75f, y + 0.35f, 0.91f));
            Stud(root, new Vector3(x + radius * 0.75f, y + 0.35f, 0.91f));
            Stud(root, new Vector3(x - radius * 0.75f, y + 0.95f, 0.91f));
            Stud(root, new Vector3(x + radius * 0.75f, y + 0.95f, 0.91f));

            if (doubleDoor)
            {
                Beam(root, "Çift Kapı Ayrımı", new Vector3(x, y, 0.98f), new Vector3(x, y + stem + radius * 0.9f, 0.98f), 0.045f, 0.09f, wood);
                Ellipsoid(root, "Sol Halka Tokmak", new Vector3(x - 0.08f, y + 0.65f, 0.92f), Vector3.one * 0.055f, iron);
                Ellipsoid(root, "Sağ Halka Tokmak", new Vector3(x + 0.08f, y + 0.65f, 0.92f), Vector3.one * 0.055f, iron);
            }
            else
            {
                Ellipsoid(root, "Demir Kapı Kolu", new Vector3(x + radius * 0.5f, y + 0.65f, 0.92f), Vector3.one * 0.06f, iron);
            }
        }

        private static void Arch(Transform parent, float x, float y, float radius, float stem, Material mat)
        {
            Beam(parent, "Kemer sol direği", new Vector3(x - radius, y, 1.04f), new Vector3(x - radius, y + stem, 1.04f), 0.11f, 0.17f, mat);
            Beam(parent, "Kemer sağ direği", new Vector3(x + radius, y, 1.04f), new Vector3(x + radius, y + stem, 1.04f), 0.11f, 0.17f, mat);
            for (int i = 0; i < 14; i++)
            {
                float a = i * Mathf.PI / 14, b = (i + 1) * Mathf.PI / 14;
                Beam(parent, "Oyma kemer parçası", new Vector3(x + Mathf.Cos(a) * radius, y + stem + Mathf.Sin(a) * radius, 1.04f), new Vector3(x + Mathf.Cos(b) * radius, y + stem + Mathf.Sin(b) * radius, 1.04f), 0.105f, 0.17f, mat);
            }
        }

        private static void WallPanel(Transform parent, float left, float right, float bottom, float top)
        {
            Polygon(parent, "Sıvalı duvar", new List<Vector3> { new Vector3(left, bottom, 1.18f), new Vector3(right, bottom, 1.18f), new Vector3(right, top, 1.18f), new Vector3(left, top, 1.18f) }, plaster);
        }

        private static void Polygon(Transform parent, string name, List<Vector3> points, Material material)
        {
            var triangles = new List<int>();
            for (int i = 1; i < points.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i + 1);
                triangles.Add(i);
            }
            MeshObject(parent, name, points.ToArray(), triangles.ToArray(), material);
        }

        private static void Strip(Transform parent, string name, List<Vector3> points, Material material)
        {
            var triangles = new List<int>();
            for (int i = 0; i < points.Count - 2; i += 2)
            {
                triangles.AddRange(new[] { i, i + 1, i + 2, i + 1, i + 3, i + 2 });
            }
            MeshObject(parent, name, points.ToArray(), triangles.ToArray(), material);
        }

        private static void MeshObject(Transform parent, string name, Vector3[] vertices, int[] triangles, Material material)
        {
            string path = RootPath + "Meshes/L1_Wall_" + (meshIndex++) + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            bool fresh = mesh == null;
            if (fresh) mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++) uv[i] = new Vector2((vertices[i].x + 3.5f) / 7.0f, (vertices[i].y - Floors[0]) / 6.5f);
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (fresh) AssetDatabase.CreateAsset(mesh, path);
            else EditorUtility.SetDirty(mesh);
            var go = Node(parent, name);
            go.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void Beam(Transform parent, string name, Vector3 a, Vector3 b, float width, float depth, Material material)
        {
            var go = Node(parent, name);
            go.localPosition = (a + b) * 0.5f;
            go.localRotation = Quaternion.FromToRotation(Vector3.right, (b - a).normalized);
            go.localScale = new Vector3(Vector3.Distance(a, b), width, depth);
            go.gameObject.AddComponent<MeshFilter>().sharedMesh = beamMesh;
            go.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static Mesh CreateBeam()
        {
            string path = RootPath + "Meshes/L1_HandHewnBeam.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null) return mesh;
            mesh = new Mesh { name = "Hand hewn bevelled timber" };
            var v = new List<Vector3>();
            var uv = new List<Vector2>();
            var t = new List<int>();
            var ring = new[] { new Vector2(-0.36f, -0.5f), new Vector2(0.36f, -0.5f), new Vector2(0.5f, -0.36f), new Vector2(0.5f, 0.36f), new Vector2(0.36f, 0.5f), new Vector2(-0.36f, 0.5f), new Vector2(-0.5f, 0.36f), new Vector2(-0.5f, -0.36f) };
            for (int r = 0; r < 4; r++) for (int j = 0; j < 8; j++)
            {
                float jitter = 1 + Mathf.Sin(r * 7 + j * 3) * 0.055f;
                v.Add(new Vector3(r / 3f - 0.5f, ring[j].x * jitter + Mathf.Sin(r * 2) * 0.035f, ring[j].y * jitter));
                uv.Add(new Vector2(r / 3f, j / 7f));
            }
            for (int r = 0; r < 3; r++) for (int j = 0; j < 8; j++)
            {
                int a = r * 8 + j, b = r * 8 + (j + 1) % 8;
                t.AddRange(new[] { a, b, a + 8, b, b + 8, a + 8 });
            }
            for (int j = 1; j < 7; j++)
            {
                t.AddRange(new[] { 0, j + 1, j, 24, 24 + j, 24 + j + 1 });
            }
            mesh.SetVertices(v);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(t, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }

        private static Transform Ellipsoid(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = material;
            return go.transform;
        }

        private static void Stud(Transform parent, Vector3 pos) => Ellipsoid(parent, "Dövme demir perçin", pos, new Vector3(0.065f, 0.065f, 0.035f), iron);

        private static GameObject Prop(string path, Transform parent, string name, Vector3 center, Vector3 size, Material material = null, Quaternion? modelRotation = null)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException(path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.name = name;
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            foreach (var c in instance.GetComponentsInChildren<Collider>()) c.enabled = false;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            bool isStylArts = path.StartsWith(StylArts, StringComparison.Ordinal);
            Renderer sizeRenderer = null;
            foreach (var r in renderers)
            {
                if (isStylArts && r.name.EndsWith("_ConvexHulls", StringComparison.Ordinal))
                {
                    r.enabled = false;
                    continue;
                }
                if (sizeRenderer == null && (!isStylArts || r.name.EndsWith("_LOD0", StringComparison.Ordinal))) sizeRenderer = r;
            }
            if (sizeRenderer == null) throw new InvalidOperationException("Görünür model bulunamadı: " + path);
            var bounds = sizeRenderer.bounds;
            if (!isStylArts) foreach (var r in renderers) bounds.Encapsulate(r.bounds);

            Vector3 baseScale = new Vector3(
                size.x / Mathf.Max(0.001f, bounds.size.x),
                size.y / Mathf.Max(0.001f, bounds.size.y),
                size.z / Mathf.Max(0.001f, bounds.size.z));
            instance.transform.localScale = baseScale;
            Vector3 offset = -Vector3.Scale(bounds.center, baseScale);

            instance.transform.SetParent(parent, false);
            instance.transform.localRotation = modelRotation ?? Quaternion.identity;
            instance.transform.localPosition = center + (modelRotation.HasValue ? modelRotation.Value * offset : offset);

            if (material != null) foreach (var r in renderers)
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = material;
                r.sharedMaterials = mats;
            }
            if (isStylArts) foreach (var group in instance.GetComponentsInChildren<LODGroup>(true))
            {
                var lods = group.GetLODs();
                for (int i = 0; i < lods.Length; i++) lods[i].screenRelativeTransitionHeight = 0.001f * (lods.Length - i);
                group.SetLODs(lods);
                group.RecalculateBounds();
            }
            return instance;
        }

        private static Material FarmMaterial()
        {
            var material = Mat("L1_FarmHouse_Atlas_URP", new Color(0.84f, 0.77f, 0.69f));
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FarmHouse/T_BC.PNG");
            var normalPath = "Assets/FarmHouse/T_NM.PNG";
            var normalImporter = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
            {
                normalImporter.textureType = TextureImporterType.NormalMap;
                normalImporter.SaveAndReimport();
            }
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            if (baseMap == null || normal == null) throw new InvalidOperationException("FarmHouse atlas dokuları bulunamadı.");
            material.SetTexture("_BaseMap", baseMap);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 0.85f);
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", 0.12f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material RealisticBedMaterial()
        {
            const string textures = "Assets/Realistic_Bed/Bed/Textures/";
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(textures + "bed1_bed_mat_AlbedoTransparency.png");
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(textures + "bed1_bed_mat_Normal.png");
            if (baseMap == null || normal == null) throw new InvalidOperationException("Realistic Bed 01 dokuları bulunamadı.");
            var material = Mat("L1_RealisticBed01_URP", Color.white);
            material.SetTexture("_BaseMap", baseMap);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 0.70f);
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", 0.18f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material SewerLampMaterial()
        {
            string matPath = RootPath + "Materials/L1_SewerHangingLamp_URP.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            var baseCol = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_basecolor.png");
            var normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_normal.png");
            var emissive = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_Cloth_Lights_Emissive.png");
            mat.SetTexture("_BaseMap", baseCol);
            if (normTex != null)
            {
                mat.SetTexture("_BumpMap", normTex);
                mat.SetFloat("_BumpScale", 0.8f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (emissive != null)
            {
                mat.SetTexture("_EmissionMap", emissive);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(2.5f, 1.6f, 0.6f));
            }
            mat.SetFloat("_Metallic", 0.15f);
            mat.SetFloat("_Smoothness", 0.30f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material SewerLampGlassMaterial()
        {
            string matPath = RootPath + "Materials/L1_SewerHangingLamp_Glass_URP.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, matPath);
            }
            mat.shader = Shader.Find("Universal Render Pipeline/Lit");
            var baseCol = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_basecolor.png");
            var normTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_normal.png");
            var emissive = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Ata Khani/Modular Sewer Props/Textures/Cloth_Lights_Cloth_Lights_Emissive.png");
            mat.SetTexture("_BaseMap", baseCol);
            if (normTex != null)
            {
                mat.SetTexture("_BumpMap", normTex);
                mat.SetFloat("_BumpScale", 0.8f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (emissive != null)
            {
                mat.SetTexture("_EmissionMap", emissive);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(3.5f, 2.2f, 0.9f));
            }
            mat.SetFloat("_Metallic", 0.05f);
            mat.SetFloat("_Smoothness", 0.65f);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material MixPbrMaterial(string name, string sourceFolder, string texturePrefix)
        {
            string basePath = sourceFolder + "/" + texturePrefix + "_BaseColor.png";
            string sourceNormal = sourceFolder + "/" + texturePrefix + "_Normal.png";
            var baseMap = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
            if (baseMap == null) throw new InvalidOperationException("MIX mobilya dokusu bulunamadı: " + basePath);
            string normalPath = RootPath + "Art/" + name + "_Normal.png";
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath) == null)
            {
                if (!AssetDatabase.CopyAsset(sourceNormal, normalPath)) throw new InvalidOperationException("MIX normal haritası kopyalanamadı: " + sourceNormal);
            }
            var importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.NormalMap)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
            string materialPath = RootPath + "Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.color = Color.white;
            material.SetTexture("_BaseMap", baseMap);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 0.65f);
            material.SetFloat("_Metallic", 0.03f);
            material.SetFloat("_Smoothness", 0.18f);
            material.EnableKeyword("_NORMALMAP");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material StylArtsMaterial(string name, Color tint, string diffuseFile = null, string normalFile = null)
        {
            var material = Mat(name, tint);
            var textureFolder = StylArts + "Textures/";
            if (diffuseFile != null)
            {
                var diffusePath = textureFolder + diffuseFile;
                var diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath);
                if (diffuse == null)
                {
                    AssetDatabase.ImportAsset(diffusePath, ImportAssetOptions.ForceSynchronousImport);
                    diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(diffusePath);
                }
                if (diffuse == null) throw new InvalidOperationException("StylArts doku bulunamadı: " + diffusePath);
                var diffuseImporter = AssetImporter.GetAtPath(diffusePath) as TextureImporter;
                if (diffuseImporter != null && diffuseImporter.maxTextureSize > 2048)
                {
                    diffuseImporter.maxTextureSize = 2048;
                    diffuseImporter.SaveAndReimport();
                }
                material.SetTexture("_BaseMap", diffuse);
            }
            else material.SetTexture("_BaseMap", null);
            if (normalFile != null)
            {
                var normalPath = textureFolder + normalFile;
                var importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                if (importer == null)
                {
                    AssetDatabase.ImportAsset(normalPath, ImportAssetOptions.ForceSynchronousImport);
                    importer = AssetImporter.GetAtPath(normalPath) as TextureImporter;
                }
                if (importer == null) throw new InvalidOperationException("StylArts normal haritası bulunamadı: " + normalPath);
                if (importer.textureType != TextureImporterType.NormalMap || importer.maxTextureSize > 2048)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    importer.maxTextureSize = 2048;
                    importer.SaveAndReimport();
                }
                material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath));
                material.SetFloat("_BumpScale", 0.55f);
                material.EnableKeyword("_NORMALMAP");
            }
            material.SetFloat("_Metallic", 0.03f);
            material.SetFloat("_Smoothness", 0.22f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static GameObject FarmPart(Transform parent, string name, string model, Vector3 center, Vector3 size, bool turnToFaceCamera = true)
        {
            var path = Farm + model + ".FBX";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("FarmHouse modeli bulunamadı: " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;
            instance.transform.localRotation = (turnToFaceCamera ? Quaternion.Euler(0, 90, 0) : Quaternion.identity) * source.transform.localRotation;
            foreach (var collider in instance.GetComponentsInChildren<Collider>()) collider.enabled = false;
            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new InvalidOperationException("FarmHouse mesh görünmüyor: " + path);
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            instance.transform.localScale = turnToFaceCamera
                ? new Vector3(size.z / Mathf.Max(0.001f, bounds.size.z), size.x / Mathf.Max(0.001f, bounds.size.x), size.y / Mathf.Max(0.001f, bounds.size.y))
                : new Vector3(size.x / Mathf.Max(0.001f, bounds.size.x), size.z / Mathf.Max(0.001f, bounds.size.z), size.y / Mathf.Max(0.001f, bounds.size.y));
            bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            instance.transform.position += parent.TransformPoint(center) - bounds.center;
            foreach (var renderer in renderers)
            {
                var mats = renderer.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = farmMaterial;
                renderer.sharedMaterials = mats;
            }
            return instance;
        }

        private static void FarmTimber(Transform parent, string name, string model, Vector3 start, Vector3 end, float thickness)
        {
            var path = Farm + model + ".FBX";
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) throw new InvalidOperationException("FarmHouse kirişi bulunamadı: " + path);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source, parent);
            instance.name = name;
            instance.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, (end - start).normalized) * source.transform.localRotation;
            var mesh = source.GetComponentInChildren<MeshFilter>();
            if (mesh == null) throw new InvalidOperationException("FarmHouse kiriş mesh'i bulunamadı: " + path);
            var size = mesh.sharedMesh.bounds.size;
            instance.transform.localScale = new Vector3(thickness / Mathf.Max(0.001f, size.x), Vector3.Distance(start, end) / Mathf.Max(0.001f, size.y), thickness / Mathf.Max(0.001f, size.z));
            var renderer = instance.GetComponentInChildren<Renderer>();
            instance.transform.localPosition = (start + end) * 0.5f - parent.InverseTransformPoint(renderer.bounds.center) + instance.transform.localPosition;
            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = farmMaterial;
                r.sharedMaterials = mats;
            }
            foreach (var c in instance.GetComponentsInChildren<Collider>()) c.enabled = false;
        }

        private static void PrepareSalvagedBoards()
        {
            string meshPath = RootPath + "Meshes/L1_SalvagedPlanks.asset";
            salvagedBoards = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (salvagedBoards == null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ata Khani/Modular Sewer Props/Prefabs/Wooden_Plank_01.prefab");
                if (prefab != null)
                {
                    salvagedBoards = UnityEngine.Object.Instantiate(prefab.GetComponentInChildren<MeshFilter>().sharedMesh);
                    salvagedBoards.name = "Salvaged wood — Ata Khani";
                    AssetDatabase.CreateAsset(salvagedBoards, meshPath);
                }
            }
            salvagedWood = Mat("L1_3D_SalvagedWood", new Color(0.85f, 0.70f, 0.49f));
        }

        private static void SalvagedPatch(Transform parent, Vector3 position, Vector3 size)
        {
            if (salvagedBoards == null) return;
            var root = Node(parent, "Asset — aşınmış ahşap döşeme");
            var b = salvagedBoards.bounds;
            var scale = new Vector3(size.x / b.size.x, size.y / b.size.y, size.z / b.size.z);
            root.localScale = scale;
            root.localPosition = position - Vector3.Scale(b.center, scale);
            root.gameObject.AddComponent<MeshFilter>().sharedMesh = salvagedBoards;
            root.gameObject.AddComponent<MeshRenderer>().sharedMaterial = salvagedWood;
        }

        private static void EnsureCrackedPlasterTextures()
        {
            string albedoPath = RootPath + "Art/L1_CrackedPlaster_BC.png";
            string normalPath = RootPath + "Art/L1_CrackedPlaster_NM.png";
            if (File.Exists(albedoPath) && File.Exists(normalPath)) return;
        }

        private static Material Mat(string name, Color color, bool texture = false)
        {
            var path = RootPath + "Materials/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.color = color;
            mat.SetFloat("_Smoothness", 0.10f);
            mat.SetFloat("_Cull", 0);
            if (texture) mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(RootPath + "Art/L1_OakFloor_Diffuse.tga"));
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Transform Node(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static GameObject Root(string name)
        {
            foreach (var r in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                if (r.name == name) return r;
            throw new InvalidOperationException(name);
        }

        private static void Folder(string path)
        {
            path = path.TrimEnd('/');
            if (AssetDatabase.IsValidFolder(path)) return;
            int i = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, i), path.Substring(i + 1));
        }

        private static SpriteRenderer Sprite(Transform parent, string name, string file, Vector3 pos, float scale, int order)
        {
            var go = Node(parent, name);
            go.localPosition = pos;
            go.localScale = Vector3.one * scale;
            var r = go.gameObject.AddComponent<SpriteRenderer>();
            r.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RootPath + "Art/" + file);
            r.sortingOrder = order;
            return r;
        }

        [MenuItem("Son Denge/Level1/3D Ev Onizleme Kaydet")]
        public static void Capture()
        {
            var camera = Root("Main Camera").GetComponent<Camera>();
            var old = camera.targetTexture;
            var active = RenderTexture.active;
            var rt = RenderTexture.GetTemporary(1920, 1080, 24);
            var image = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = rt;
                camera.Render();
                RenderTexture.active = rt;
                image.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
                image.Apply();
                File.WriteAllBytes(RootPath + "Art/L1_Playable3D_Preview.png", image.EncodeToPNG());
                AssetDatabase.ImportAsset(RootPath + "Art/L1_Playable3D_Preview.png");
            }
            finally
            {
                camera.targetTexture = old;
                RenderTexture.active = active;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(image);
            }
        }
    }
}
