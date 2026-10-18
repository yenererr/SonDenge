using System.Collections.Generic;
using System.IO;
using Jam.Levels.Level1;
using Jam.Missions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Jam.EditorTools
{
    /// <summary>
    /// Beş bölüm sahnesini Level1 şablonundan kurar: her bölümün görev eşyaları (low-poly prop),
    /// hedef alanları, E ile kullanılan noktaları ve L1_MissionTask bağlantıları gerçek sahne
    /// nesneleri olarak oluşturulur. Tekrar çalıştırılabilir; önceki "[Görevler]" grubunu yeniler.
    /// Menü: Jam > Wilson > ...
    /// </summary>
    public static class WilsonSceneBuilder
    {
        private const string Level1Path = "Assets/Scenes/Level1.unity";
        private const string MatDir = "Assets/Levels/Level1/Materials/Missions";
        private const string WilsonDir = "Assets/Levels/Level1/Art/Characters/Wilson";
        private const string ExtinguisherPrefab = "Assets/AK STUDIO ART/Fire Extinguisher/Prefabs/Fire Extinguisher.prefab";
        private static readonly float[] Floor = { -1.15f, 1.10f, 3.35f };
        private const float PropZ = 0.25f;

        private static Transform house, furnitureRoot, group;
        private static L1_ProjectGameController game;

        [MenuItem("Jam/Wilson/Level1 görevlerini kur (açık sahne)")]
        public static void BuildLevel1InOpenScene() => SetupOpenScene(1);

        [MenuItem("Jam/Wilson/Level2-5 sahnelerini Level1'den üret")]
        public static void BuildOtherScenes()
        {
            for (var n = 2; n <= 5; n++) BuildSceneFromLevel1(n);
            EditorSceneManager.OpenScene(Level1Path);
        }

        /// <summary>Level1'i açar, görev grubunu bölüm n için kurar ve LevelN.unity olarak kaydeder (meta GUID korunur).</summary>
        public static void BuildSceneFromLevel1(int n)
        {
            var scene = EditorSceneManager.OpenScene(Level1Path, OpenSceneMode.Single);
            SetupOpenScene(n);
            var path = "Assets/Scenes/Level" + n + ".unity";
            EditorSceneManager.SaveScene(scene, path, true);
            Debug.Log("[Wilson] " + path + " kaydedildi.");
        }

        public static void SetupOpenScene(int n)
        {
            game = Object.FindFirstObjectByType<L1_ProjectGameController>();
            if (game == null) { Debug.LogError("[Wilson] L1_ProjectGameController bulunamadı."); return; }
            house = game.houseRoot;
            furnitureRoot = house.Find("Bağımsız 3D mobilyalar") ?? house;
            Directory.CreateDirectory(MatDir);

            // Önceki görev kurulumunu kaldır.
            foreach (var old in house.GetComponentsInChildren<Transform>(true))
                if (old != null && old.name.StartsWith("[Görevler]")) Object.DestroyImmediate(old.gameObject);
            foreach (var prop in furnitureRoot.GetComponentsInChildren<L1_MovableProp>(true))
                if (prop != null && prop.name.StartsWith("[Görev eşyası]")) Object.DestroyImmediate(prop.gameObject);

            var levelAsset = AssetDatabase.LoadAssetAtPath<LevelDefinition>(LevelPath(n));
            game.levelDefinition = levelAsset;
            game.missionMode = true;
            game.autoStart = true;
            game.growDownward = false;
            // Görev sahneleri: karakter uçtan uca yürüyünce ~14° (0,0057'de 23° sınırına dayanıyordu),
            // hazırlıkta kayma yalnız 8° üstünde ve çok yavaş.
            game.torqueSensitivity = 0.002f;
            game.planningMaxSlideSpeed = 0.06f;
            game.planningMinSlideTiltDeg = 8f;
            game.wilsonModel = AssetDatabase.LoadAssetAtPath<GameObject>(WilsonDir + "/low_poly_chicken.obj");
            game.wilsonEggModel = AssetDatabase.LoadAssetAtPath<GameObject>(WilsonDir + "/egg.obj");
            game.wilsonTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(WilsonDir + "/low_poly_chicken.png");
            if (game.GetComponent<MissionManager>() == null) game.gameObject.AddComponent<MissionManager>();
            game.missions = game.GetComponent<MissionManager>();
            var mm = new SerializedObject(game.missions);
            mm.FindProperty("level").objectReferenceValue = levelAsset;
            mm.ApplyModifiedPropertiesWithoutUndo();
            var director = game.GetComponent<L1_MissionDirector>() ?? game.gameObject.AddComponent<L1_MissionDirector>();
            var so = new SerializedObject(director);
            so.FindProperty("game").objectReferenceValue = game;
            so.FindProperty("drag").objectReferenceValue = game.drag;
            so.ApplyModifiedPropertiesWithoutUndo();
            if (game.drag != null) { game.drag.allowCarry = true; game.drag.carryOnlyMarked = true; EditorUtility.SetDirty(game.drag); }

            // Balkon yalnız fırtına bölümünde görünür.
            var balcony = house.Find("Ahşap balkon ve vinç");
            if (balcony != null) balcony.gameObject.SetActive(n == 2);

            group = new GameObject("[Görevler] Bölüm " + n).transform;
            group.SetParent(house, false);
            switch (n)
            {
                case 1: Level1(); break;
                case 2: Level2(); break;
                case 3: Level3(); break;
                case 4: Level4(); break;
                case 5: Level5(); break;
            }
            var tasks = new SerializedObject(director);
            var list = group.GetComponentsInChildren<L1_MissionTask>(true);
            var arr = tasks.FindProperty("tasks");
            arr.arraySize = list.Length;
            for (var i = 0; i < list.Length; i++) arr.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            tasks.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
            if (n == 1) EditorSceneManager.SaveScene(game.gameObject.scene);
            Debug.Log("[Wilson] Bölüm " + n + " görevleri kuruldu: " + list.Length + " görev.");
        }

        private static string LevelPath(int n) => "Assets/Data/Levels/" +
            (n == 1 ? "L1_Deprem" : n == 2 ? "L2_Firtina" : n == 3 ? "L3_Yangin" : n == 4 ? "L4_Yildirim" : "L5_Yagmur") + ".asset";

        // ================================================================ Bölüm 1 — Deprem
        private static void Level1()
        {
            var bookshelf = FindProp("kitaplık");
            var fridge = FindProp("buzdolabı");
            Task("L1_T01", L1_MissionTask.Condition.Anchored, new[] { bookshelf });
            Task("L1_T02", L1_MissionTask.Condition.Anchored, new[] { fridge });
            var bag = Prop("Deprem çantası", 10f, new Vector3(-1.75f, Floor[0], 0f), new Vector3(0.42f, 0.48f, 0.3f), BagVisual);
            var bagZone = Zone("EmergencyBagZone", new Vector3(3.05f, Floor[0], 0f), new Vector3(0.95f, 1.2f, 2.4f));
            Task("L1_T03", L1_MissionTask.Condition.ItemsInZone, new[] { bag }, bagZone);
            Task("L1_T04", L1_MissionTask.Condition.Balance, new L1_MovableProp[0]);
        }

        // ================================================================ Bölüm 2 — Fırtına
        private static void Level2()
        {
            var brace = Switch("RoofBraceAnchor", new Vector3(-3.3f, Floor[1] + 0.9f, -0.2f), "ÇATI BAĞLANTISINI TAK",
                "Çatı bağlantısı takıldı.", false);
            BraceVisual(brace);
            Task("L2_T01", L1_MissionTask.Condition.Switches, null, null, new[] { brace });
            var w1 = Window("Pencere alt kat sol", new Vector3(-2.48f, Floor[0], 0f));
            var w2 = Window("Pencere orta kat sol", new Vector3(-1.98f, Floor[1], 0f));
            var w3 = Window("Pencere orta kat sağ", new Vector3(1.98f, Floor[1], 0f));
            Task("L2_T02", L1_MissionTask.Condition.Switches, null, null, new[] { w1, w2, w3 });
            var potA = Prop("Balkon saksısı", 8f, new Vector3(3.75f, Floor[1], -0.55f), new Vector3(0.36f, 0.5f, 0.36f), PotVisual);
            var potB = Prop("Balkon saksısı (küçük)", 6f, new Vector3(4.15f, Floor[1], -0.55f), new Vector3(0.3f, 0.42f, 0.3f), PotVisual);
            var storage = Zone("StormSafeStorageZone", new Vector3(1.2f, Floor[1], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            Task("L2_T03", L1_MissionTask.Condition.ItemsInZone, new[] { potA, potB }, storage);
            var torch = Prop("El feneri", 2f, new Vector3(-2.6f, Floor[1], 0f), new Vector3(0.36f, 0.2f, 0.2f), FlashlightVisual);
            var torchZone = Zone("FlashlightZone", new Vector3(1.0f, Floor[0], 0f), new Vector3(0.7f, 1.2f, 2.4f));
            Task("L2_T04", L1_MissionTask.Condition.ItemsInZone, new[] { torch }, torchZone);
        }

        // ================================================================ Bölüm 3 — Yangın
        private static void Level3()
        {
            var alarm = Prop("Duman alarmı", 1f, new Vector3(-3.6f, Floor[0], 0f), new Vector3(0.3f, 0.12f, 0.3f), AlarmVisual);
            var mount = Zone("SmokeAlarmMountZone", new Vector3(0.9f, Floor[1], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            var install = Switch("SmokeAlarmInstall", new Vector3(0.9f, Floor[1] + 1.1f, -0.3f), "ALARMI KUR",
                "Alarm tavana takıldı. Şimdi E ile test et!", false);
            install.requiresItemInZone = mount;
            install.requiredItem = alarm;
            install.consumeItem = true;
            MountedAlarmVisual(install, Floor[1] + 2.05f);
            var test = Switch("SmokeAlarmTest", new Vector3(1.35f, Floor[1] + 1.1f, -0.3f), "ALARMI TEST ET", "Bip bip! Alarm çalışıyor.", false);
            test.requiresSwitch = install;
            TestLightVisual(test);
            Task("L3_T01", L1_MissionTask.Condition.Switches, null, mount, new[] { install, test });

            var stove = Switch("StoveKnob", new Vector3(0.25f, Floor[1] + 0.75f, -0.3f), "OCAĞI KAPAT", "Ocak kapandı.", false);
            StoveVisual(stove);
            Task("L3_T02", L1_MissionTask.Condition.Switches, null, null, new[] { stove });

            var ext = Prop("Yangın tüpü", 10f, new Vector3(-3.4f, Floor[1], 0f), new Vector3(0.3f, 0.62f, 0.3f), ExtinguisherVisual);
            var hook = Zone("ExtinguisherMountZone", new Vector3(3.35f, Floor[0], 0f), new Vector3(0.75f, 1.2f, 2.4f));
            Task("L3_T03", L1_MissionTask.Condition.ItemsInZone, new[] { ext }, hook);

            var box1 = Prop("Koli (merdiven önü)", 12f, new Vector3(0.95f, Floor[0], 0f), new Vector3(0.55f, 0.45f, 0.45f), BoxVisual);
            var box2 = Prop("Koli (merdiven önü, küçük)", 8f, new Vector3(1.43f, Floor[0], 0f), new Vector3(0.42f, 0.38f, 0.4f), BoxVisual);
            var exit = Zone("ExitPathVolume", new Vector3(1.2f, Floor[0], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            exit.markerColor = new Color(0.85f, 0.35f, 0.3f, 0.85f);
            Zone("ClearStorageZone", new Vector3(-3.9f, Floor[0], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            Task("L3_T04", L1_MissionTask.Condition.ZoneClear, new[] { box1, box2 }, exit);
        }

        // ================================================================ Bölüm 4 — Yıldırım
        private static void Level4()
        {
            var control = Switch("LightningSystemControlPoint", new Vector3(1.6f, Floor[2] + 0.9f, -0.2f),
                "BAĞLANTIYI TAMAMLA", "Yıldırım koruma bağlantısı tamam.", false);
            ControlBoxVisual(control);
            Task("L4_T01", L1_MissionTask.Condition.Switches, null, null, new[] { control });
            var tvPlug = Switch("TVPlug", new Vector3(-0.2f, Floor[0] + 0.25f, -0.25f), "TV FİŞİNİ ÇEK", "TV fişten çekildi.", true);
            PlugVisual(tvPlug);
            DecorComputer(new Vector3(2.9f, Floor[1], 0.35f));
            var pcPlug = Switch("ComputerPlug", new Vector3(3.5f, Floor[1] + 0.25f, -0.25f), "BİLGİSAYAR FİŞİNİ ÇEK",
                "Bilgisayar fişten çekildi.", true);
            PlugVisual(pcPlug);
            Task("L4_T02", L1_MissionTask.Condition.Switches, null, null, new[] { tvPlug, pcPlug });
            var surge = Prop("Gerilim koruyucu", 2f, new Vector3(-3.5f, Floor[1], 0f), new Vector3(0.42f, 0.12f, 0.16f), SurgeVisual);
            var zone = Zone("SurgeProtectorZone", new Vector3(3.1f, Floor[1], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            Task("L4_T03", L1_MissionTask.Condition.ItemsInZone, new[] { surge }, zone);
            // Pencereler afette tehlike noktasıdır (davranış puanı).
            HazardMarker("Pencere (tehlike)", new Vector3(-2.48f, Floor[0] + 1.0f, -0.2f));
            HazardMarker("Pencere (tehlike)", new Vector3(1.98f, Floor[1] + 1.0f, -0.2f));
        }

        // ================================================================ Bölüm 5 — Şiddetli yağmur
        private static void Level5()
        {
            // Kapı önünü boşalt: çeyiz sandığı bu bölümde orta katta durur.
            var chest = FindProp("sandık");
            if (chest != null) chest.transform.localPosition = new Vector3(3.6f, Floor[1] + 0.27f, PropZ);
            var bags = new List<L1_MovableProp>();
            for (var i = 0; i < 2; i++)
                bags.Add(Prop("Kum torbası " + (i + 1), 15f, new Vector3(-3.4f + i * 0.7f, Floor[1], 0f),
                    new Vector3(0.5f, 0.28f, 0.4f), SandbagVisual));
            var barrier = Zone("FrontDoorBarrierZone", new Vector3(3.55f, Floor[0], 0f), new Vector3(1.5f, 1.2f, 2.4f));
            Task("L5_T01", L1_MissionTask.Condition.ItemsInZone, bags.ToArray(), barrier);
            var drain = Switch("DrainageGrate", new Vector3(2.0f, Floor[0] + 0.15f, -0.4f), "DRENAJI TEMİZLE", "Drenaj açıldı.", false);
            DrainVisual(drain);
            Task("L5_T02", L1_MissionTask.Condition.Switches, null, null, new[] { drain });
            var radio = Prop("Eski radyo", 6f, new Vector3(-1.75f, Floor[0], 0f), new Vector3(0.42f, 0.3f, 0.25f), RadioVisual);
            var album = Prop("Fotoğraf albümü kutusu", 5f, new Vector3(-0.12f, Floor[0], 0f), new Vector3(0.3f, 0.22f, 0.3f), AlbumVisual);
            var dry = Zone("DryStorageZone", new Vector3(2.4f, Floor[2], 0f), new Vector3(1.4f, 1.2f, 2.4f));
            Task("L5_T03", L1_MissionTask.Condition.ItemsInZone, new[] { radio, album }, dry);
            var kit = Prop("Conta / yalıtım kiti", 3f, new Vector3(-2.9f, Floor[1], 0f), new Vector3(0.36f, 0.24f, 0.24f), KitVisual);
            var leakZone = Zone("SealLeakZone", new Vector3(1.05f, Floor[0], 0f), new Vector3(0.9f, 1.2f, 2.4f));
            var seal = Switch("SealLeak", new Vector3(1.4f, Floor[0] + 1.0f, -0.3f), "CONTAYI UYGULA", "Sızıntı kapandı.", false);
            seal.requiresItemInZone = leakZone;
            seal.requiredItem = kit;
            seal.consumeItem = true;
            LeakVisual(seal);
            Task("L5_T04", L1_MissionTask.Condition.Switches, null, leakZone, new[] { seal });
        }

        // ================================================================ yapı taşları
        private static L1_MovableProp FindProp(string key)
        {
            foreach (var p in house.GetComponentsInChildren<L1_MovableProp>(true))
                if (p.displayName != null && p.displayName.ToLowerInvariant().Contains(key)) return p;
            Debug.LogWarning("[Wilson] Eşya bulunamadı: " + key);
            return null;
        }

        private static L1_MissionTask Task(string id, L1_MissionTask.Condition condition, L1_MovableProp[] items,
            L1_MissionZone zone = null, L1_MissionSwitch[] switches = null)
        {
            var go = new GameObject("Görev " + id);
            go.transform.SetParent(group, false);
            var t = go.AddComponent<L1_MissionTask>();
            t.taskId = id;
            t.condition = condition;
            t.items = items ?? new L1_MovableProp[0];
            t.zone = zone;
            t.switches = switches ?? new L1_MissionSwitch[0];
            return t;
        }

        private static L1_MissionZone Zone(string id, Vector3 floorPoint, Vector3 size)
        {
            var go = new GameObject("Hedef — " + id);
            go.transform.SetParent(group, false);
            go.transform.localPosition = new Vector3(floorPoint.x, floorPoint.y + size.y * 0.5f, PropZ);
            var z = go.AddComponent<L1_MissionZone>();
            z.zoneId = id;
            z.size = size;
            return z;
        }

        private static L1_MissionSwitch Switch(string id, Vector3 local, string action, string done, bool hazard)
        {
            var go = new GameObject("Görev noktası — " + id);
            go.transform.SetParent(group, false);
            go.transform.localPosition = local;
            var s = go.AddComponent<L1_MissionSwitch>();
            s.switchId = id;
            s.actionTr = action;
            s.doneTr = done;
            s.hazardDuringDisaster = hazard;
            var on = new GameObject("Etkin görsel");
            on.transform.SetParent(go.transform, false);
            var off = new GameObject("Etkin değil görsel");
            off.transform.SetParent(go.transform, false);
            s.onVisual = on;
            s.offVisual = off;
            // Küçük sarı "E" işareti: oyuncu noktayı bulabilsin.
            var tag = Part(off.transform, PrimitiveType.Sphere, new Vector3(0f, 0.55f, -0.15f), new Vector3(0.14f, 0.14f, 0.14f), Mat("Isaret", new Color(1f, 0.82f, 0.25f), true));
            tag.name = "E işareti";
            return s;
        }

        private static L1_MovableProp Prop(string displayName, float mass, Vector3 floorPoint, Vector3 size,
            System.Action<Transform, Vector3> visual)
        {
            var go = new GameObject("[Görev eşyası] " + displayName);
            go.transform.SetParent(furnitureRoot, false);
            // floorPoint.z == 0 ise standart eşya derinliği (0.25); balkon gibi yerlerde verilen z kullanılır.
            go.transform.localPosition = new Vector3(floorPoint.x, floorPoint.y + size.y * 0.5f + 0.02f,
                Mathf.Approximately(floorPoint.z, 0f) ? PropZ : floorPoint.z);
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            var body = go.AddComponent<Rigidbody>();
            body.mass = Mathf.Clamp(mass / 6f, 0.5f, 4f);
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var prop = go.AddComponent<L1_MovableProp>();
            prop.displayName = displayName;
            prop.itemId = "gorev_" + displayName.ToLowerInvariant().Replace(' ', '_');
            prop.balanceMassKg = mass;
            prop.carryable = true;
            var v = new GameObject("Görsel");
            v.transform.SetParent(go.transform, false);
            visual(v.transform, size);
            return prop;
        }

        private static GameObject Part(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat,
            Vector3? euler = null)
        {
            var p = GameObject.CreatePrimitive(type);
            Object.DestroyImmediate(p.GetComponent<Collider>());
            p.transform.SetParent(parent, false);
            p.transform.localPosition = pos;
            p.transform.localScale = scale;
            if (euler.HasValue) p.transform.localEulerAngles = euler.Value;
            p.GetComponent<Renderer>().sharedMaterial = mat;
            return p;
        }

        private static Material Mat(string name, Color color, bool emissive = false, float smooth = 0.25f)
        {
            var path = MatDir + "/L1_M_" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.SetColor("_BaseColor", color);
            m.SetFloat("_Smoothness", smooth);
            if (emissive)
            {
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * 1.4f);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------------------------------------------------------------- görseller (low-poly)
        private static void BagVisual(Transform v, Vector3 s)
        {
            var red = Mat("CantaKirmizi", new Color(0.78f, 0.16f, 0.13f));
            var dark = Mat("CantaKoyu", new Color(0.45f, 0.08f, 0.07f));
            var white = Mat("Beyaz", new Color(0.95f, 0.94f, 0.9f));
            Part(v, PrimitiveType.Cube, Vector3.zero, s, red);
            Part(v, PrimitiveType.Cube, new Vector3(0f, s.y * 0.32f, -s.z * 0.52f), new Vector3(s.x * 0.92f, s.y * 0.34f, 0.04f), dark);
            Part(v, PrimitiveType.Cube, new Vector3(0f, -s.y * 0.05f, -s.z * 0.53f), new Vector3(0.16f, 0.05f, 0.02f), white);
            Part(v, PrimitiveType.Cube, new Vector3(0f, -s.y * 0.05f, -s.z * 0.53f), new Vector3(0.05f, 0.16f, 0.02f), white);
            Part(v, PrimitiveType.Cylinder, new Vector3(0f, s.y * 0.58f, 0f), new Vector3(0.05f, s.x * 0.28f, 0.05f), dark, new Vector3(0f, 0f, 90f));
        }

        private static void FlashlightVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cylinder, new Vector3(-0.04f, 0f, 0f), new Vector3(0.11f, 0.14f, 0.11f), Mat("FenerSari", new Color(0.95f, 0.75f, 0.12f)), new Vector3(0f, 0f, 90f));
            Part(v, PrimitiveType.Cylinder, new Vector3(0.14f, 0f, 0f), new Vector3(0.16f, 0.05f, 0.16f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)), new Vector3(0f, 0f, 90f));
            Part(v, PrimitiveType.Cylinder, new Vector3(0.19f, 0f, 0f), new Vector3(0.13f, 0.01f, 0.13f), Mat("Lens", new Color(1f, 0.97f, 0.8f), true), new Vector3(0f, 0f, 90f));
        }

        private static void AlarmVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.28f, 0.05f, 0.28f), Mat("Beyaz", new Color(0.95f, 0.94f, 0.9f)));
            Part(v, PrimitiveType.Sphere, new Vector3(0.06f, 0.05f, -0.05f), Vector3.one * 0.035f, Mat("LedKirmizi", new Color(0.95f, 0.15f, 0.1f), true));
        }

        private static void MountedAlarmVisual(L1_MissionSwitch s, float ceilingY)
        {
            var y = ceilingY - s.transform.localPosition.y;
            Part(s.onVisual.transform, PrimitiveType.Cylinder, new Vector3(0f, y, 0.3f), new Vector3(0.3f, 0.05f, 0.3f), Mat("Beyaz", new Color(0.95f, 0.94f, 0.9f)));
            Part(s.onVisual.transform, PrimitiveType.Sphere, new Vector3(0.07f, y - 0.04f, 0.25f), Vector3.one * 0.035f, Mat("LedKirmizi", new Color(0.95f, 0.15f, 0.1f), true));
            // Montaj noktası halkası (henüz takılmadı).
            Part(s.offVisual.transform, PrimitiveType.Cylinder, new Vector3(0f, y + 0.01f, 0.3f), new Vector3(0.34f, 0.01f, 0.34f), Mat("Isaret", new Color(1f, 0.82f, 0.25f), true));
        }

        private static void TestLightVisual(L1_MissionSwitch s)
        {
            Part(s.onVisual.transform, PrimitiveType.Sphere, new Vector3(-0.45f, 0.98f, 0.3f), Vector3.one * 0.07f, Mat("LedYesil", new Color(0.2f, 0.9f, 0.35f), true));
        }

        private static void StoveVisual(L1_MissionSwitch s)
        {
            var flame = Mat("Alev", new Color(1f, 0.45f, 0.08f), true);
            Part(s.offVisual.transform, PrimitiveType.Sphere, new Vector3(-0.35f, 0.37f, 0.45f), new Vector3(0.18f, 0.08f, 0.18f), flame);
            Part(s.offVisual.transform, PrimitiveType.Sphere, new Vector3(-0.1f, 0.37f, 0.45f), new Vector3(0.14f, 0.07f, 0.14f), flame);
            Part(s.onVisual.transform, PrimitiveType.Cylinder, new Vector3(-0.2f, 0.36f, 0.45f), new Vector3(0.2f, 0.005f, 0.2f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)));
        }

        private static void ExtinguisherVisual(Transform v, Vector3 s)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ExtinguisherPrefab);
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, v);
                foreach (var c in inst.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
                foreach (var rb in inst.GetComponentsInChildren<Rigidbody>(true)) Object.DestroyImmediate(rb);
                var b = new Bounds(); var has = false;
                foreach (var r in inst.GetComponentsInChildren<Renderer>()) { if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds); }
                if (has && b.size.y > 0.001f)
                {
                    inst.transform.localScale *= s.y / b.size.y;
                    b = new Bounds(); has = false;
                    foreach (var r in inst.GetComponentsInChildren<Renderer>()) { if (!has) { b = r.bounds; has = true; } else b.Encapsulate(r.bounds); }
                    inst.transform.position += v.position - b.center;
                }
                return;
            }
            Part(v, PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.22f, s.y * 0.48f, 0.22f), Mat("CantaKirmizi", new Color(0.78f, 0.16f, 0.13f)));
        }

        private static void BoxVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cube, Vector3.zero, s, Mat("Karton", new Color(0.72f, 0.55f, 0.34f)));
            Part(v, PrimitiveType.Cube, new Vector3(0f, s.y * 0.5f, 0f), new Vector3(s.x * 1.01f, 0.01f, 0.08f), Mat("Bant", new Color(0.86f, 0.78f, 0.6f)));
        }

        private static void PotVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cylinder, new Vector3(0f, -s.y * 0.2f, 0f), new Vector3(s.x * 0.9f, s.y * 0.3f, s.z * 0.9f), Mat("Terrakota", new Color(0.72f, 0.36f, 0.22f)));
            Part(v, PrimitiveType.Sphere, new Vector3(0f, s.y * 0.22f, 0f), new Vector3(s.x * 1.05f, s.y * 0.6f, s.z * 1.05f), Mat("Yaprak", new Color(0.3f, 0.55f, 0.28f)));
        }

        private static void SurgeVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cube, Vector3.zero, new Vector3(s.x, s.y * 0.7f, s.z), Mat("Beyaz", new Color(0.95f, 0.94f, 0.9f)));
            for (var i = 0; i < 3; i++)
                Part(v, PrimitiveType.Cube, new Vector3(-0.12f + i * 0.12f, s.y * 0.36f, 0f), new Vector3(0.07f, 0.01f, 0.07f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)));
            Part(v, PrimitiveType.Sphere, new Vector3(s.x * 0.42f, s.y * 0.36f, 0f), Vector3.one * 0.03f, Mat("LedYesil", new Color(0.2f, 0.9f, 0.35f), true));
        }

        private static void SandbagVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Sphere, Vector3.zero, new Vector3(s.x, s.y * 1.05f, s.z), Mat("Cuval", new Color(0.74f, 0.66f, 0.46f), false, 0.05f));
            Part(v, PrimitiveType.Cylinder, new Vector3(s.x * 0.42f, 0f, 0f), new Vector3(0.06f, 0.03f, 0.06f), Mat("Ip", new Color(0.5f, 0.4f, 0.25f)), new Vector3(0f, 0f, 90f));
        }

        private static void RadioVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cube, Vector3.zero, s, Mat("Ahsap", new Color(0.5f, 0.32f, 0.18f)));
            Part(v, PrimitiveType.Cylinder, new Vector3(-0.1f, 0f, -s.z * 0.52f), new Vector3(0.16f, 0.01f, 0.16f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)), new Vector3(90f, 0f, 0f));
            Part(v, PrimitiveType.Cylinder, new Vector3(0.14f, 0.05f, -s.z * 0.52f), new Vector3(0.05f, 0.01f, 0.05f), Mat("Pirinc", new Color(0.85f, 0.68f, 0.3f)), new Vector3(90f, 0f, 0f));
        }

        private static void AlbumVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cube, Vector3.zero, s, Mat("Mavi", new Color(0.2f, 0.36f, 0.6f)));
            Part(v, PrimitiveType.Cube, new Vector3(0f, 0f, -s.z * 0.51f), new Vector3(s.x * 0.5f, s.y * 0.4f, 0.01f), Mat("Krem", new Color(0.96f, 0.93f, 0.85f)));
        }

        private static void KitVisual(Transform v, Vector3 s)
        {
            Part(v, PrimitiveType.Cube, Vector3.zero, s, Mat("Gri", new Color(0.45f, 0.48f, 0.5f)));
            Part(v, PrimitiveType.Cube, new Vector3(0f, s.y * 0.62f, 0f), new Vector3(s.x * 0.5f, 0.04f, 0.04f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)));
            Part(v, PrimitiveType.Cube, new Vector3(0f, 0f, -s.z * 0.51f), new Vector3(s.x * 0.6f, s.y * 0.25f, 0.01f), Mat("FenerSari", new Color(0.95f, 0.75f, 0.12f)));
        }

        private static void BraceVisual(L1_MissionSwitch s)
        {
            var metal = Mat("Metal", new Color(0.55f, 0.57f, 0.6f), false, 0.6f);
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(0f, 0.95f, 0.4f), new Vector3(1.4f, 0.08f, 0.08f), metal);
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(-0.45f, 0.6f, 0.4f), new Vector3(0.08f, 0.8f, 0.08f), metal, new Vector3(0f, 0f, 35f));
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(0.45f, 0.6f, 0.4f), new Vector3(0.08f, 0.8f, 0.08f), metal, new Vector3(0f, 0f, -35f));
            Part(s.offVisual.transform, PrimitiveType.Cube, new Vector3(0f, 0.95f, 0.4f), new Vector3(1.4f, 0.03f, 0.03f), Mat("Isaret", new Color(1f, 0.82f, 0.25f), true));
        }

        /// <summary>Kepenkli pencere: açıkken kanatlar yana yatık, kapalıyken pencerenin önünde.</summary>
        private static L1_MissionSwitch Window(string name, Vector3 floorPoint)
        {
            var s = Switch(name, new Vector3(floorPoint.x, floorPoint.y + 1.0f, -0.3f), "PENCEREYİ KAPAT", "Pencere kilitlendi.", true);
            var wood = Mat("Kepenk", new Color(0.42f, 0.27f, 0.16f));
            for (var i = 0; i < 2; i++)
            {
                var side = i == 0 ? -1f : 1f;
                Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(side * 0.36f, 0.35f, 1.35f), new Vector3(0.7f, 1.2f, 0.06f), wood);
                Part(s.offVisual.transform, PrimitiveType.Cube, new Vector3(side * 0.98f, 0.35f, 1.1f), new Vector3(0.7f, 1.2f, 0.06f), wood, new Vector3(0f, side * 70f, 0f));
            }
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(0f, 0.35f, 1.31f), new Vector3(0.12f, 0.18f, 0.06f), Mat("Metal", new Color(0.55f, 0.57f, 0.6f), false, 0.6f));
            return s;
        }

        private static void ControlBoxVisual(L1_MissionSwitch s)
        {
            Part(s.transform, PrimitiveType.Cube, new Vector3(0f, 0.2f, 0.5f), new Vector3(0.5f, 0.6f, 0.15f), Mat("Gri", new Color(0.45f, 0.48f, 0.5f)));
            Part(s.offVisual.transform, PrimitiveType.Cube, new Vector3(0.1f, 0.25f, 0.4f), new Vector3(0.06f, 0.3f, 0.06f), Mat("CantaKirmizi", new Color(0.78f, 0.16f, 0.13f)), new Vector3(0f, 0f, -35f));
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(0.1f, 0.3f, 0.4f), new Vector3(0.06f, 0.3f, 0.06f), Mat("LedYesil", new Color(0.2f, 0.9f, 0.35f), true));
            Part(s.transform, PrimitiveType.Cylinder, new Vector3(0f, 1.5f, 0.5f), new Vector3(0.04f, 1.0f, 0.04f), Mat("Bakir", new Color(0.72f, 0.45f, 0.2f), false, 0.5f));
        }

        private static void PlugVisual(L1_MissionSwitch s)
        {
            var white = Mat("Beyaz", new Color(0.95f, 0.94f, 0.9f));
            var black = Mat("Siyah", new Color(0.12f, 0.12f, 0.13f));
            Part(s.transform, PrimitiveType.Cube, new Vector3(0f, 0f, 0.6f), new Vector3(0.16f, 0.16f, 0.04f), white);
            Part(s.offVisual.transform, PrimitiveType.Cube, new Vector3(0f, 0f, 0.55f), new Vector3(0.09f, 0.09f, 0.08f), black);
            Part(s.offVisual.transform, PrimitiveType.Cylinder, new Vector3(-0.25f, -0.12f, 0.55f), new Vector3(0.025f, 0.25f, 0.025f), black, new Vector3(0f, 0f, 60f));
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(-0.3f, -0.22f, 0.5f), new Vector3(0.09f, 0.05f, 0.09f), black);
        }

        private static void DecorComputer(Vector3 floorPoint)
        {
            var root = new GameObject("Bilgisayar (dekor)").transform;
            root.SetParent(group, false);
            root.localPosition = floorPoint;
            var wood = Mat("Ahsap", new Color(0.5f, 0.32f, 0.18f));
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0.38f, 0f), new Vector3(1.0f, 0.06f, 0.5f), wood);
            Part(root, PrimitiveType.Cube, new Vector3(-0.42f, 0.18f, 0f), new Vector3(0.06f, 0.36f, 0.45f), wood);
            Part(root, PrimitiveType.Cube, new Vector3(0.42f, 0.18f, 0f), new Vector3(0.06f, 0.36f, 0.45f), wood);
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0.68f, 0.05f), new Vector3(0.55f, 0.36f, 0.04f), Mat("Siyah", new Color(0.12f, 0.12f, 0.13f)));
            Part(root, PrimitiveType.Cube, new Vector3(0f, 0.68f, 0.02f), new Vector3(0.5f, 0.3f, 0.01f), Mat("Ekran", new Color(0.3f, 0.55f, 0.75f), true));
        }

        private static void DrainVisual(L1_MissionSwitch s)
        {
            var metal = Mat("Metal", new Color(0.55f, 0.57f, 0.6f), false, 0.6f);
            for (var i = 0; i < 5; i++)
                Part(s.transform, PrimitiveType.Cube, new Vector3(-0.24f + i * 0.12f, -0.13f, 0.6f), new Vector3(0.04f, 0.02f, 0.5f), metal);
            var mud = Mat("Camur", new Color(0.33f, 0.24f, 0.14f), false, 0.1f);
            Part(s.offVisual.transform, PrimitiveType.Sphere, new Vector3(-0.1f, -0.1f, 0.55f), new Vector3(0.3f, 0.08f, 0.3f), mud);
            Part(s.offVisual.transform, PrimitiveType.Sphere, new Vector3(0.15f, -0.1f, 0.7f), new Vector3(0.22f, 0.07f, 0.25f), Mat("Yaprak", new Color(0.3f, 0.55f, 0.28f)));
        }

        private static void LeakVisual(L1_MissionSwitch s)
        {
            var water = Mat("Su", new Color(0.35f, 0.6f, 0.9f), true, 0.9f);
            Part(s.offVisual.transform, PrimitiveType.Cube, new Vector3(0f, -0.35f, 1.3f), new Vector3(0.05f, 0.7f, 0.02f), water);
            Part(s.offVisual.transform, PrimitiveType.Sphere, new Vector3(0f, -0.95f, 1.2f), new Vector3(0.5f, 0.02f, 0.4f), water);
            Part(s.onVisual.transform, PrimitiveType.Cube, new Vector3(0f, -0.3f, 1.3f), new Vector3(1.3f, 0.06f, 0.03f), Mat("Conta", new Color(0.15f, 0.15f, 0.16f)));
        }

        private static void HazardMarker(string name, Vector3 local)
        {
            var s = Switch(name, local, "", "", true);
            s.interactable = false;
            Object.DestroyImmediate(s.offVisual);
            Object.DestroyImmediate(s.onVisual);
            s.onVisual = null;
            s.offVisual = null;
        }
    }
}
