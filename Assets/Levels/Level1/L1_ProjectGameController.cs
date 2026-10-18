using System;
using System.Collections.Generic;
using Jam.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    [Serializable]
    public sealed class L1_ProjectSpriteEntry
    {
        public string id;
        public Sprite sprite;
    }

    /// <summary>
    /// Unity adaptation of the five-stage rules in project-files/App.tsx.
    /// The existing house, resident and 3D furniture remain the playable world.
    /// </summary>
    public sealed partial class L1_ProjectGameController : MonoBehaviour
    {
        private enum RoundState { StartMenu, Planning, Disaster, Report, GameOver, Victory }

        private sealed class ShopItem
        {
            public readonly string id, title, category, template;
            public readonly int cost, unlockLevel, mass;
            public ShopItem(string id, string title, string category, int cost, int unlockLevel, int mass = 0, string template = null)
            { this.id = id; this.title = title; this.category = category; this.cost = cost; this.unlockLevel = unlockLevel; this.mass = mass; this.template = template; }
        }

        // Her katın eşyaları ağırlıkça iki eşit gruba bölünebilir; ev matematiksel olarak dengelenebilir.
        // Kat 1 (280): kitaplık 80 + kanepe 60 = sandık 40 + koltuk 40 + TV 20 + sehpa 20 + komodin 20
        // Kat 2 (240): buzdolabı 80 + fıçı 40 = ocak 70 + masa 50
        // Kat 3 (140): yatak 70 = küvet 50 + lavabo 20
        private static readonly (string key, int kg)[] BalancedWeights =
        {
            ("kitaplık", 80), ("kanepe", 60), ("sandık", 40), ("tekli koltuk", 40), ("televizyon", 20),
            ("sehpa", 20), ("komodin", 20), ("buzdolabı", 80), ("fıçı", 40), ("ocak", 70),
            ("yemek masası", 50), ("yatak", 70), ("küvet", 50), ("lavabo", 20),
        };

        private static int BalancedWeight(string displayName)
        {
            var name = (displayName ?? string.Empty).ToLowerInvariant();
            foreach (var entry in BalancedWeights)
                if (name.Contains(entry.key)) return entry.kg;
            return 0;
        }

        private sealed class FireSpot
        {
            public GameObject visual;
            public GameObject smoke;
            public L1_MovableProp target;
            public float health;
            public float spreadClock;
            public float burnClock;
            public int floor;
            public float x;
        }

        private sealed class HangingLamp
        {
            public Transform mount;
            public Quaternion restingRotation;
            public float phase;
            public float sway;
            public float swayVelocity;
        }

        private static readonly string[] Categories = { "Mobilya", "Deprem", "Fırtına", "Yangın", "Yıldırım", "Yağmur" };
        private static readonly string[] LevelNames = { "", "Deprem", "Fırtına", "Yangın", "Yıldırım", "Şiddetli Yağmur" };
        private static readonly float[] FloorY = { -1.15f, 1.10f, 3.35f };
        private static readonly ShopItem[] Catalog =
        {
            new ShopItem("green_sofa", "Adaçayı koltuk", "Mobilya", 150, 1, 60, "MIX — yıpranmış deri kanepe"),
            new ShopItem("red_armchair", "Tekli koltuk", "Mobilya", 90, 1, 40, "StylArts — orta kat tekli koltuk"),
            new ShopItem("coffee_table", "Ahşap sehpa", "Mobilya", 60, 1, 20, "MIX — sehpa"),
            new ShopItem("bookshelf", "Kitaplık", "Mobilya", 200, 1, 80, "MIX — dokulu kitaplık"),
            new ShopItem("refrigerator", "Retro buzdolabı", "Mobilya", 220, 2, 80, "StylArts — buzdolabı"),
            new ShopItem("stove_counter", "Ocak ve fırın", "Mobilya", 190, 2, 70, "StylArts — ocak ve fırın"),
            new ShopItem("dining_table", "Yemek masası", "Mobilya", 130, 2, 50, "Yemek masası"),
            new ShopItem("wooden_barrel", "Erzak fıçısı", "Mobilya", 100, 2, 40, "Erzak fıçısı"),
            new ShopItem("cozy_bed", "Çift kişilik yatak", "Mobilya", 175, 3, 70, "Realistic Bed 01 — yatak"),
            new ShopItem("wardrobe", "Gardırop", "Mobilya", 210, 3, 60),
            new ShopItem("bedside_table", "Komodin ve şamdan", "Mobilya", 50, 3, 20, "Komodin"),
            new ShopItem("standing_lamp", "Pirinç lambader", "Mobilya", 45, 1, 20),
            new ShopItem("deprem_cantasi", "Deprem çantası", "Deprem", 80, 1, 10),
            new ShopItem("sabitleme_aparati", "Mobilya sabitleme", "Deprem", 30, 1, 2),
            new ShopItem("storm_brace", "Çatı güçlendirici", "Fırtına", 50, 2, 10),
            new ShopItem("yangin_tupu", "Yangın tüpü", "Yangın", 90, 1, 10),
            new ShopItem("duman_dedektoru", "Duman dedektörü", "Yangın", 40, 2, 1),
            new ShopItem("lightning_rod_system", "Paratoner", "Yıldırım", 45, 3, 12),
            new ShopItem("surge_protector", "Gerilim koruyucu", "Yıldırım", 20, 1, 4),
            new ShopItem("gutter_system", "Yağmur oluğu", "Yağmur", 25, 3, 8),
            new ShopItem("drainage_channel", "Drenaj kanalı", "Yağmur", 35, 1, 25),
            new ShopItem("waterproof_sealing", "Su yalıtımı", "Yağmur", 20, 1, 3),
            new ShopItem("sandbags", "Kum torbaları", "Yağmur", 15, 1, 40),
            new ShopItem("water_pump", "Tahliye pompası", "Yağmur", 40, 1, 15),
            new ShopItem("water_sensor", "Su sensörü", "Yağmur", 15, 1, 2),
        };

        public Transform houseRoot;
        public Transform balancePivot;
        public L1_ResidentController resident;
        public L1_PropDrag drag;
        public Material projectSpriteMaterial;
        public L1_ProjectSpriteEntry[] spriteEntries;
        public AudioClip planningMusic;
        public AudioClip disasterMusic;
        [Tooltip("Eski tek katlı deprem raporu kurallarını kullanır; eşya yerleşimi açık katlar arasından seçilir.")]
        public bool singleFloorLevel = true;
        [Tooltip("Odanın iç yarı genişliği (m). Zemin collider'ı ile aynı olmalı; L1_PropDrag'deki değerle eşleşmeli.")]
        public float roomHalfWidth = 4.3f;
        [Tooltip("Level 1 iki katlı: ikinci seviye mobilyaları markette erken açılır.")]
        public bool kitchenUpperFloor = true;

        private bool IsLocked(ShopItem item) => level < item.unlockLevel &&
            !(kitchenUpperFloor && item.unlockLevel == 2 && item.category == "Mobilya");

        [Tooltip("Açıksa eğim, başlangıç dizilimine göre değil gerçek tork toplamına göre hesaplanır.")]
        public bool absoluteTorque = false;
        [Tooltip("Hazırlıkta mevcut eşyalar kaymaz; yeni yerleştirilenler 1,5 saniye sonra eğime tepki verir.")]
        public bool holdFurnitureWhilePlanning = true;
        [Tooltip("Eğimde eşyaları kaydıran ek ivme (m/sn²) ve en yüksek kayma hızı. Panel oyunu: 420 px/sn² ≈ 4.4.")]
        public float tiltSlideAccel = 1.5f;
        public float maxSlideSpeed = 0.35f;
        [Tooltip("Hazırlıkta (Hold Furniture While Planning açıkken) eşyaların en yüksek kayma hızı (m/sn). 0 = hiç kaymaz.")]
        public float planningMaxSlideSpeed = 0.12f;
        [Tooltip("Bu açının (derece) altında sürtünme eşyayı tutar; deprem sarsıntısı yine iter.")]
        public float minSlideTiltDeg = 4f;
        [Tooltip("Hazırlıkta kaymanın başladığı açı (derece). Küçük eğimde eşyalar yerinde kalır, kısır döngü oluşmaz.")]
        public float planningMinSlideTiltDeg = 8f;
        [Tooltip("Eşya ağırlığının evi yatırabileceği en büyük açı (derece). Devrilme açısının altında kalırsa ev ancak afetin ekstra itişiyle devrilir.")]
        public float maxTargetTiltDeg = 23f;
        [Tooltip("Odanın iki yanındaki görünmez duvarların merkezden uzaklığı (m). Eşyalar evden dökülmez.")]
        public float sideWallInnerX = 5.55f;
        [Tooltip("Devrilme açısı (derece) ve bu açıda kesintisiz kalma süresi (saniye).")]
        public float collapseAngle = 26f;
        public float collapseHoldSeconds = 1f;
        [Tooltip("Tork -> eğim çarpanı. Büyüdükçe ev daha kolay eğilir.")]
        public float torqueSensitivity = 0.0057f; // panel 0.00006 / (8.6 m / 820 px)
        [Header("Afet süreleri ve bütçe")]
        public float stormDuration = 12f;
        public float fireMaxTime = 30f;
        public float lightningDuration = 25f;
        public float rainDuration = 25f;
        [Tooltip("Her bölüm geçişinde bütçeye eklenen sabit bonus.")]
        public int budgetBonusPerLevel = 450;
        [Range(0f, 1f)] public float defaultMusicVolume = 0.5f;
        private float collapseClock;
        [Tooltip("Açıksa bölüm, ana menüden gelindiği için kendi başlangıç menüsünü göstermeden başlar.")]
        public bool autoStart = true;
        [Tooltip("Eski IMGUI üst bar/uyarı/seçim çubuğu. Yeni uGUI HUD (L1_Hud) varken kapalı tutun.")]
        public bool useLegacyHud = false;

        private readonly HashSet<string> equipment = new HashSet<string>();
        private readonly List<FireSpot> fires = new List<FireSpot>();
        private readonly List<L1_MovableProp> furniture = new List<L1_MovableProp>();
        private readonly Dictionary<string, L1_MovableProp> furnitureTemplates = new Dictionary<string, L1_MovableProp>();
        private readonly List<HangingLamp> hangingLamps = new List<HangingLamp>();
        private PhysicsMaterial looseFurnitureMaterial, mediumFurnitureMaterial, heavyFurnitureMaterial;
        private GameObject foamVisual;
        private AudioSource music;
        private L1_ProjectSfx sfx;
        private RoundState state = RoundState.StartMenu;
        private int level = 1, budget = 750, score, anchors;
        private float timer = 45f, houseAngle, angularVelocity, baselineTorque, smoothedTorque;
        private bool levelEntryBalanceLocked;
        [SerializeField, Min(0f)] private float residentBalanceMassKg = 25f; // panel: 65 kg * 0.38
        [SerializeField, Min(0f)] private float residentMovementThreshold = 0.12f;
        private float levelEntryResidentX;
        private float maxTilt;
        private float buildingHealth = 100f, water, indoorFlood, lightningClock, lightningFlash;
        private float evacuatedWater, waterSensorCooldown;
        private int extinguishedFires, fireSpreads, lightningHits, lightningBlocked;
        private float warningUntil;
        private string warning, report;
        private string fireCause;
        private bool marketOpen;
        private bool menuOpen, howToOpen, musicMuted;
        private int categoryIndex;
        private Vector2 marketScroll;
        private Transform upperFloor, upperLadder, upperExtras, roof;
        private Camera sceneCamera;
        private static bool startImmediatelyOnReload;

        private void Awake()
        {
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = defaultMusicVolume;
            var effects = new GameObject("Proje dosyası efekt sesleri");
            effects.transform.SetParent(transform, false);
            sfx = effects.AddComponent<L1_ProjectSfx>();
            sceneCamera = Camera.main;
            SetState(RoundState.StartMenu);
        }

        private void Start()
        {
            upperFloor = houseRoot.Find("Kat 3");
            upperLadder = houseRoot.Find("Orta kattan çatı katına");
            upperExtras = houseRoot.Find("[L1] Üçüncü kat balkon ve vinç parçaları");
            roof = houseRoot.Find("Asimetrik kiremit çatı");
            SetUpperFloorVisible(false);
            if (drag != null) drag.floorTopsProvider = FloorTops;
            if (drag != null) drag.ManipulationStarted += ReleaseLevelEntryBalance;
            BuildSideWalls();
            RefreshFurniture();
            CacheFurnitureTemplates();
            CacheHangingLamps();
            PrepareExtinguisherModel();
            SpawnWilson();
            baselineTorque = ComputeTorque();
            smoothedTorque = baselineTorque;
            if (startImmediatelyOnReload || autoStart)
            {
                startImmediatelyOnReload = false;
                BeginGame();
            }
        }

        private void RefreshFurniture()
        {
            furniture.Clear();
            if (houseRoot != null) furniture.AddRange(houseRoot.GetComponentsInChildren<L1_MovableProp>(true));
            if (looseFurnitureMaterial == null)
            {
                looseFurnitureMaterial = FurnitureSurface("L1 kolay kayan mobilya", 0.10f, 0.06f);
                mediumFurnitureMaterial = FurnitureSurface("L1 orta tutunma", 0.22f, 0.14f);
                heavyFurnitureMaterial = FurnitureSurface("L1 ağır mobilya tutunması", 0.32f, 0.22f);
            }
            foreach (var item in furniture)
            {
                if (item == null) continue;
                var balanced = BalancedWeight(item.displayName);
                if (balanced > 0) item.balanceMassKg = balanced;
                else if (item.balanceMassKg <= 0f)
                {
                    var catalogMass = 0;
                    foreach (var entry in Catalog)
                    {
                        if ((item.itemId != null && item.itemId.StartsWith(entry.id)) ||
                            (entry.template != null && item.displayName != null &&
                             item.displayName.Contains(entry.template)))
                        { catalogMass = entry.mass; break; }
                    }
                    // Additional 3D room props have no counterpart in the 2D catalog.
                    item.balanceMassKg = catalogMass > 0 ? catalogMass :
                        item.Body != null ? Mathf.Max(10f, item.Body.mass * 10f) : 10f;
                }
                var collider = item.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    var name = (item.displayName ?? string.Empty).ToLowerInvariant();
                    collider.sharedMaterial = name.Contains("sehpa") || name.Contains("komodin") ||
                        name.Contains("koltuk") || name.Contains("sandık")
                        ? looseFurnitureMaterial
                        : name.Contains("buzdolabı") || name.Contains("ocak") ||
                          name.Contains("kitaplık") || name.Contains("yatak") || name.Contains("küvet")
                            ? heavyFurnitureMaterial : mediumFurnitureMaterial;
                }
                if (item.Body != null) item.Body.linearDamping = 0.85f;
                if (item.gameObject.activeInHierarchy) item.AllowFallAtEdges(houseRoot);
            }
            IgnoreFurnitureCollisions();
        }

        // Eşyalar birbirini itmez; her biri kendi başına kayar. Biri kilitlenince diğerleri yine serbest.
        private void IgnoreFurnitureCollisions()
        {
            var colliders = new List<Collider>();
            foreach (var item in furniture)
                if (item != null) colliders.AddRange(item.GetComponentsInChildren<Collider>(true));
            for (var i = 0; i < colliders.Count; i++)
                for (var j = i + 1; j < colliders.Count; j++)
                    if (colliders[i].attachedRigidbody != colliders[j].attachedRigidbody)
                        UnityEngine.Physics.IgnoreCollision(colliders[i], colliders[j], true);
        }

        private void CacheFurnitureTemplates()
        {
            var root = new GameObject("Mobilya model şablonları");
            root.transform.SetParent(transform, false);
            root.SetActive(false);
            foreach (var entry in Catalog)
            {
                if (entry.category != "Mobilya" || entry.template == null) continue;
                foreach (var item in furniture)
                {
                    if (item == null || item.displayName == null || !item.displayName.Contains(entry.template)) continue;
                    var copy = Instantiate(item.gameObject, root.transform).GetComponent<L1_MovableProp>();
                    copy.name = entry.title + " model şablonu";
                    furnitureTemplates[entry.id] = copy;
                    break;
                }
            }
        }

        private static PhysicsMaterial FurnitureSurface(string name, float staticFriction, float dynamicFriction)
        {
            return new PhysicsMaterial(name)
            {
                staticFriction = staticFriction,
                dynamicFriction = dynamicFriction,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f
            };
        }

        private void CacheHangingLamps()
        {
            hangingLamps.Clear();
            if (houseRoot == null) return;
            foreach (var mount in houseRoot.GetComponentsInChildren<Transform>(true))
            {
                if (mount.name != "Modular Sewer — Sarkan Lamba") continue;
                hangingLamps.Add(new HangingLamp
                {
                    mount = mount,
                    restingRotation = mount.localRotation,
                    phase = hangingLamps.Count * 1.73f
                });
            }
        }

        private void LateUpdate()
        {
            // Unity can reload scripts while Play Mode remains open. Start does not
            // always run again, so recover the mounts when the runtime list is empty.
            if (hangingLamps.Count == 0) CacheHangingLamps();
            if (hangingLamps.Count == 0) return;
            // Mounts inherit the house tilt. Each hanging lamp also moves around
            // its ceiling attachment, including a small visible idle motion.
            var houseMotion = Mathf.Clamp(-angularVelocity * Mathf.Rad2Deg * 0.55f, -9f, 9f);
            foreach (var lamp in hangingLamps)
            {
                if (lamp.mount == null) continue;
                var idle = Mathf.Sin(Time.unscaledTime * 1.65f + lamp.phase) * (state == RoundState.Disaster ? 8f : 5.5f);
                // Ev eğilse de lamba yer çekimiyle dik sarkar: evin açısını geri çevir.
                var plumb = -houseAngle * Mathf.Rad2Deg;
                var target = plumb + Mathf.Clamp(houseMotion + idle, -16f, 16f);
                lamp.sway = Mathf.SmoothDamp(lamp.sway, target, ref lamp.swayVelocity,
                    0.22f, Mathf.Infinity, Time.unscaledDeltaTime);
                lamp.mount.localRotation = lamp.restingRotation * Quaternion.Euler(0f, 0f, lamp.sway);
            }
        }

        private void SetUpperFloorVisible(bool visible)
        {
            if (upperFloor != null) upperFloor.gameObject.SetActive(visible);
            if (upperLadder != null) upperLadder.gameObject.SetActive(visible);
            if (upperExtras != null) upperExtras.gameObject.SetActive(visible);
            if (roof != null) roof.localPosition = new Vector3(visible ? -0.70f : 0.55f,
                visible ? 0f : -2.25f, 0f);
            foreach (var prop in houseRoot.GetComponentsInChildren<L1_MovableProp>(true))
            {
                var local = houseRoot.InverseTransformPoint(prop.transform.position);
                if (local.y >= 2.5f) prop.gameObject.SetActive(visible);
            }
            if (sceneCamera != null)
            {
                var position = sceneCamera.transform.position;
                sceneCamera.transform.position = new Vector3(position.x, visible ? 2.75f : 1.65f, position.z);
                sceneCamera.orthographicSize = visible ? 5f : 4.1f;
            }
        }

        private void BeginGame()
        {
            level = 1;
            budget = 750;
            score = anchors = 0;
            equipment.Clear();
            ApplyLevelSetup();
            timer = PreparationSeconds;
            maxTilt = 0f;
            LockLevelEntryBalance();
            evacuatedWater = waterSensorCooldown = 0f;
            HideFlood();
            extinguishedFires = fireSpreads = lightningHits = lightningBlocked = 0;
            marketOpen = menuOpen = howToOpen = false;
            SetMusic(planningMusic);
            SetState(RoundState.Planning);
            if (sfx != null) sfx.Play("confirm");
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && state != RoundState.StartMenu && !howToOpen &&
                !PauseSubMenuOpen && !briefingOpen && !eggTipOpen &&
                !(marketOpen && !useLegacyHud))
                ToggleMenu();
            UpdateAudio(state == RoundState.StartMenu || menuOpen || howToOpen || briefingOpen || eggTipOpen || Time.timeScale <= 0f);
            if (state == RoundState.StartMenu || menuOpen || howToOpen || briefingOpen || eggTipOpen || Time.timeScale <= 0f) return;
            if (warningUntil > 0f) warningUntil -= Time.deltaTime;
            lightningFlash = Mathf.MoveTowards(lightningFlash, 0f, Time.deltaTime * 2.5f);
            if (keyboard != null)
            {
                if (keyboard.iKey.wasPressedThisFrame && !missionMode) marketOpen = !marketOpen && state == RoundState.Planning;
                if (keyboard.spaceKey.wasPressedThisFrame && state == RoundState.Disaster && resident != null)
                    ToggleCover();
                else if (keyboard.spaceKey.wasPressedThisFrame && state == RoundState.Planning && !countdownActive)
                {
                    if (DisasterRequested != null) DisasterRequested();
                    else StartDisaster();
                }
                if (keyboard.fKey.wasPressedThisFrame && state == RoundState.Planning) ToggleAnchor();
                if (keyboard.rKey.wasPressedThisFrame && (state == RoundState.GameOver || state == RoundState.Victory)) Restart();
                if (keyboard.tKey.wasPressedThisFrame && missionMode && state == RoundState.Disaster && level == 3) UiEvacuate();
            }

            if (state == RoundState.Planning && !countdownActive)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    timer = 0f;
                    // Süre bitince de afet 3-2-1 geri sayımından geçer (şartname §2.1).
                    if (missionMode && ForcedDisasterRequested != null) ForcedDisasterRequested();
                    else StartDisaster(true);
                }
            }
            else if (state == RoundState.Disaster)
            {
                timer -= Time.deltaTime;
                TickDisaster(Time.deltaTime, keyboard);
                UpdateQuakeCamera();
                if (state == RoundState.Disaster && timer <= 0f)
                {
                    if (level == 3 && fires.Count > 0)
                    {
                        // Görev modunda açık kaçış yolu varsa büyüyen yangından tahliye doğru karardır.
                        if (missionMode && equipment.Contains("exit_clear")) Evacuate();
                        else Lose(missionMode ? "Yangın büyüdü ve kaçış yolu kapalıydı." : "Yangın zamanında söndürülemedi.");
                    }
                    else CompleteLevel();
                }
            }
        }

        /// <summary>
        /// Her katın yürünebilir zemininin iki ucuna görünmez duvar kurar. Katlar simetrik değildir;
        /// duvar zeminin bittiği yerde olmazsa eşya duvara varmadan boşluktan düşer.
        /// </summary>
        private void BuildSideWalls()
        {
            foreach (var floor in houseRoot.GetComponentsInChildren<BoxCollider>(true))
            {
                if (floor.name != "Yürünebilir zemin" || floor.GetComponentInParent<L1_MovableProp>() != null) continue;
                var t = floor.transform;
                var scale = t.lossyScale;
                const float thickness = 0.3f, height = 2.3f;
                for (var side = -1; side <= 1; side += 2)
                {
                    var wall = new GameObject(side < 0 ? "[L1] Görünmez sol kenar" : "[L1] Görünmez sağ kenar");
                    wall.transform.SetParent(t, false);
                    var halfX = floor.size.x * 0.5f + thickness * 0.5f / Mathf.Abs(scale.x);
                    wall.transform.localPosition = floor.center + new Vector3(side * halfX,
                        floor.size.y * 0.5f + height * 0.5f / Mathf.Abs(scale.y), 0f);
                    wall.AddComponent<BoxCollider>().size = new Vector3(thickness / Mathf.Abs(scale.x),
                        height / Mathf.Abs(scale.y), Mathf.Max(floor.size.z, 3f / Mathf.Abs(scale.z)));
                }
            }
        }

        private void FixedUpdate()
        {
            if (balancePivot == null || houseRoot == null || state == RoundState.StartMenu ||
                state == RoundState.Report || state == RoundState.GameOver || state == RoundState.Victory ||
                growing || menuOpen || howToOpen || briefingOpen) return;
            TrackLostFurniture();
            if (state == RoundState.Planning && levelEntryBalanceLocked)
            {
                var residentX = resident != null
                    ? houseRoot.InverseTransformPoint(resident.transform.position).x : levelEntryResidentX;
                var residentShift = residentX - levelEntryResidentX;
                var currentTorque = ComputeTorque();
                if (Mathf.Abs(residentShift) < residentMovementThreshold)
                {
                    // Yeni katın pasif yerleşimi evi yatırmaz; karakter yürüyene kadar dengeyi koru.
                    baselineTorque = currentTorque;
                    smoothedTorque = baselineTorque;
                    houseAngle = angularVelocity = 0f;
                    balancePivot.localRotation = Quaternion.identity;
                    return;
                }
                // Yürüyüş kilidi açar. O anki mobilya yükünü başlangıç kabul edip yalnız
                // karakterin kat ettiği mesafeyi eğime ekle; aşağıdaki sönümleme geçerli kalır.
                baselineTorque = currentTorque - ResidentBalanceMass() * residentShift;
                smoothedTorque = currentTorque;
                ReleaseLevelEntryBalance();
            }
            var disturbance = 0f;
            var quake = 0f;
            if (state == RoundState.Disaster && level == 1)
            {
                quake = QuakeAcceleration();
                disturbance = quake * quakeTiltPerAcceleration;
            }
            else if (state == RoundState.Disaster && level == 2)
                disturbance = (equipment.Contains("storm_brace") ? 0.012f : 0.035f) * (1f + Mathf.Sin(Time.time * 1.3f)) *
                    (equipment.Contains("windows_closed") ? 0.75f : 1f) * (equipment.Contains("balcony_cleared") ? 0.85f : 1f);

            foreach (var item in furniture)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.IsAnchored || item.Body == null || item.Body.isKinematic) continue;
                var slideThreshold = state == RoundState.Planning ? Mathf.Max(minSlideTiltDeg, planningMinSlideTiltDeg) : minSlideTiltDeg;
                var heldByFriction = Mathf.Abs(houseAngle) < slideThreshold * Mathf.Deg2Rad && quake == 0f;
                var slowPlanning = holdFurnitureWhilePlanning && state == RoundState.Planning && !item.AllowPlanningSlide;
                if (heldByFriction || slowPlanning && planningMaxSlideSpeed <= 0f)
                {
                    // Hazırlıkta eğim, eşyayı kendiliğinden kaydırıp zincirleme devrilme yaratmasın.
                    var right = houseRoot.right;
                    item.Body.AddForce(-Vector3.Project(UnityEngine.Physics.gravity, right), ForceMode.Acceleration);
                    var slide = Vector3.Dot(item.Body.linearVelocity, right);
                    item.Body.AddForce(-right * slide * 8f, ForceMode.Acceleration);
                    continue;
                }
                // Gravity already pulls props down the rotated floor. A small,
                // continuous assist overcomes contact friction without a sudden threshold.
                var slope = -Mathf.Sin(houseAngle) * tiltSlideAccel;
                var shake = quake;
                var wind = state == RoundState.Disaster && level == 2 ? (equipment.Contains("storm_brace") ? 0.8f : 2.5f) : 0f;
                item.Body.AddForce(houseRoot.right * (slope + shake + wind), ForceMode.Acceleration);
                var along = Vector3.Dot(item.Body.linearVelocity, houseRoot.right);
                // Sert hız sınırı: eşya ivmelenip hızlanmaz, oyuncu yakalayıp düzenleyebilir.
                var excessSpeed = Mathf.Abs(along) - (slowPlanning ? planningMaxSlideSpeed : maxSlideSpeed);
                if (excessSpeed > 0f)
                    item.Body.linearVelocity -= houseRoot.right * (Mathf.Sign(along) * excessSpeed);
            }

            // Positive weight on the right lowers the right side: clockwise / negative Z.
            // Filter the moving furniture's torque and ignore tiny settling jitter.
            smoothedTorque = Mathf.Lerp(smoothedTorque, ComputeTorque(),
                1f - Mathf.Exp(-Time.fixedDeltaTime / 0.95f));
            var torqueDelta = smoothedTorque - (absoluteTorque ? 0f : baselineTorque);
            // A soft dead zone removes tiny settling jitter while keeping the
            // response continuous as a resident or prop crosses the center.
            torqueDelta = Mathf.Sign(torqueDelta) *
                (Mathf.Sqrt(torqueDelta * torqueDelta + 25f) - 5f);
            // Use the same limit before and during a disaster; switching states
            // must not release a previously clamped target in one step.
            var maxTarget = maxTargetTiltDeg * Mathf.Deg2Rad;
            var target = Mathf.Clamp(-torqueDelta * torqueSensitivity, -maxTarget, maxTarget) + disturbance;
            // The source game keeps the house nearly level from the fire stage
            // onward so those rounds are decided by their own hazards. Retain a
            // small visible response for the physical 3D house.
            if (level >= 3 && !missionMode) target = Mathf.Clamp(target * 0.16f, -0.065f, 0.065f);
            houseAngle = Mathf.SmoothDamp(houseAngle, target, ref angularVelocity,
                1.55f, 0.15f, Time.fixedDeltaTime);
            balancePivot.localRotation = Quaternion.Euler(0f, 0f, houseAngle * Mathf.Rad2Deg);
            maxTilt = Mathf.Max(maxTilt, Mathf.Abs(houseAngle * Mathf.Rad2Deg));
            // Anlık sıçrama oyunu bitirmez; açı eşiğin üstünde kesintisiz kalmalı.
            // Görev modunda 26° + 1 sn kuralı beş bölümde ve hazırlıkta da geçerlidir.
            var collapseArmed = missionMode ? state == RoundState.Disaster || state == RoundState.Planning
                : state == RoundState.Disaster && level <= 2;
            if (collapseArmed && Mathf.Abs(houseAngle) >= collapseAngle * Mathf.Deg2Rad)
            {
                collapseClock += Time.fixedDeltaTime;
                if (collapseClock >= collapseHoldSeconds)
                    Lose(state == RoundState.Planning ? "Denge bozuldu: ev hazırlık sırasında 26 dereceyi aştı." : "Ev denge sınırını aşıp devrildi.");
            }
            else collapseClock = 0f;
        }

        private float ComputeTorque()
        {
            if (houseRoot == null) return 0f;
            var torque = 0f;
            foreach (var item in furniture)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.Body == null) continue;
                if (drag != null && item == drag.Carried) continue;
                var local = houseRoot.InverseTransformPoint(item.Body.position);
                // Ease out a prop's load as it leaves the platform. Dropping it
                // from the sum at one coordinate made the whole house snap back.
                // Mathf.SmoothStep(a, b, t) a→b arası enterpolasyondur; eşik geçişi için
                // önce t'yi 0–1 aralığına çevirmek gerekir.
                var edgeWeight = 1f - Smooth01(roomHalfWidth - 0.08f, roomHalfWidth + 1.32f, Mathf.Abs(local.x));
                var heightWeight = Smooth01(lowestFloorTop - 1.65f, lowestFloorTop - 0.65f, local.y);
                torque += item.Mass * local.x * edgeWeight * heightWeight;
            }
            if (drag != null && drag.Carried != null && drag.Carried.Body != null)
            {
                // A carried item no longer presses on the floor, but its load
                // still acts at the resident's hand position as they walk.
                // Görev modunda taşınan kütle tam ve bir kez sayılır (şartname §2.3).
                var carriedMass = missionMode ? drag.Carried.Mass : Mathf.Max(10f, drag.Carried.Mass * 0.75f);
                torque += carriedMass * drag.CarriedLocalX;
            }
            if (resident != null)
                torque += ResidentBalanceMass() * houseRoot.InverseTransformPoint(resident.transform.position).x;
            if (wilson != null) torque += wilson.EggTorque();
            return torque;
        }

        private float ResidentBalanceMass() => residentBalanceMassKg + (bagAttached ? bagMassKg : 0f);

        private static float Smooth01(float edge0, float edge1, float value) =>
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(edge0, edge1, value));

        private void LockLevelEntryBalance()
        {
            levelEntryBalanceLocked = true;
            levelEntryResidentX = resident != null && houseRoot != null
                ? houseRoot.InverseTransformPoint(resident.transform.position).x : 0f;
            baselineTorque = ComputeTorque();
            smoothedTorque = baselineTorque;
            houseAngle = angularVelocity = 0f;
            if (balancePivot != null) balancePivot.localRotation = Quaternion.identity;
        }

        private void ReleaseLevelEntryBalance() => levelEntryBalanceLocked = false;

        private void TickDisaster(float dt, Keyboard keyboard)
        {
            if (level == 3)
            {
                TickFireDisaster(dt, keyboard);
            }
            else if (level == 4)
            {
                lightningClock -= dt;
                if (lightningClock <= 0f)
                {
                    lightningClock = UnityEngine.Random.Range(3.5f, 5f);
                    lightningFlash = 0.8f;
                    if (!equipment.Contains("lightning_rod_system"))
                    {
                        lightningHits++;
                        buildingHealth -= (equipment.Contains("surge_protector") ? 25f : 37f) *
                            (equipment.Contains("unplugged") ? 0.8f : 1f);
                        if (sfx != null && !sfx.PlayClip("thunder")) sfx.Play("strike");
                    }
                    else
                    {
                        lightningBlocked++;
                        if (sfx != null) { sfx.PlayClip("thunder", 0.6f); sfx.Play("shield"); }
                    }
                    if (buildingHealth <= 0f) Lose("Yıldırım darbeleri evi yıktı.");
                }
            }
            else if (level == 5 && missionMode) UpdateFlood(dt);
            else if (level == 5)
            {
                var evacuation = (equipment.Contains("gutter_system") ? 5.5f : 0f) +
                    (equipment.Contains("drainage_channel") ? 6f : 0f) +
                    (equipment.Contains("water_pump") ? 7f : 0f);
                var availableWater = water + 16f * dt * 2.2f;
                var actualEvacuation = Mathf.Min(evacuation * dt * 2.2f, availableWater);
                evacuatedWater += actualEvacuation;
                water = Mathf.Clamp(availableWater - actualEvacuation, 0f, 200f);
                var barrier = equipment.Contains("sandbags") ? 65f : 25f;
                waterSensorCooldown = Mathf.Max(0f, waterSensorCooldown - dt);
                if (equipment.Contains("water_sensor") && water >= barrier * 0.75f && waterSensorCooldown <= 0f)
                {
                    Warn("Su sensörü: giriş eşiğinde su yükseliyor!");
                    waterSensorCooldown = 8f;
                }
                if (water > barrier)
                    indoorFlood = Mathf.Clamp(indoorFlood + (water - barrier) * dt * 0.15f *
                        (equipment.Contains("waterproof_sealing") ? 0.25f : 1f), 0f, 100f);
                else indoorFlood = Mathf.Max(0f, indoorFlood - dt * 4f);
                if (indoorFlood >= 100f) Lose("Dağ suyu evi bastı.");
            }
        }

        private void StartDisaster(bool forced = false)
        {
            if (!CanStartDisaster(forced)) return;
            if (levelEntryBalanceLocked)
            {
                baselineTorque = ComputeTorque();
                smoothedTorque = baselineTorque;
            }
            ReleaseLevelEntryBalance();
            marketOpen = false;
            timer = DisasterSeconds;
            if (missionMode) BeginMissionDisaster();
            else if (level == 1) BeginQuakeRecord();
            if (level == 3)
            {
                extinguishedFires = fireSpreads = 0;
                BeginFireDisaster();
                var source = UnityEngine.Random.Range(0, 4);
                // Ocak kapatıldıysa ocak kaynaklı tutuşma olmaz; başka küçük bir arıza olabilir.
                if (missionMode && equipment.Contains("stove_off") && (source == 0 || source == 3)) source = UnityEngine.Random.Range(1, 3);
                var floor = Mathf.Clamp(source == 1 ? 1 : source == 2 ? 2 : 0, 0, FloorCount - 1);
                var x = source == 0 ? -0.2f : source == 1 ? 1.1f : source == 2 ? -0.7f : 0.8f;
                fireCause = source == 0 ? "Mutfakta aşırı ısınan ocak" : source == 1 ?
                    "Oturma odasında aşırı yüklü priz" : source == 2 ?
                    "Yatak odasında arızalı ısıtıcı" : "Ocakta unutulan kızgın tava";
                SpawnFire(floor, x, 100f);
                // Görev modunda tek küçük tutuşma; alarm yoksa geç fark edildiği için yayılmış başlar.
                if (!missionMode || !equipment.Contains("duman_dedektoru"))
                    SpawnFire(floor == 1 ? 0 : 1, x > 0f ? -0.8f : 0.8f, 85f);
                if (equipment.Contains("duman_dedektoru")) Warn("Duman dedektörü: yangın erken algılandı!");
            }
            if (level == 4) { buildingHealth = 100f; lightningClock = 2.5f; lightningHits = lightningBlocked = 0; }
            if (level == 5) { water = indoorFlood = evacuatedWater = waterSensorCooldown = 0f; if (missionMode) BeginFlood(); }
            SetMusic(disasterMusic);
            SetState(RoundState.Disaster);
            if (sfx != null) sfx.Play(level == 3 ? "strike" : "confirm");
        }

        private bool CanStartDisaster(bool forced)
        {
            if (state != RoundState.Planning) return false;
            if (missionMode) return true;
            if (level == 1 && !equipment.Contains("deprem_cantasi") && !forced) { Warn("Deprem çantası olmadan afeti başlatamazsın. Marketten edin."); return false; }
            if (level == 3 && !equipment.Contains("yangin_tupu") && !forced) { Warn("Yangın tüpü olmadan yangına müdahale edemezsin. Marketten edin."); return false; }
            if (level <= 2 && Mathf.Abs(houseAngle) > 18f * Mathf.Deg2Rad && !forced) { Warn("Ev çok eğik. Önce eşyaları dengele."); return false; }
            return true;
        }

        private void CompleteLevel()
        {
            if (state != RoundState.Disaster) return;
            if (missionMode) { CompleteMissionLevel(true, null); return; }
            if (level == 1 && singleFloorLevel) { CompleteEarthquakeLevel(true, null); return; }
            var anchored = 0;
            foreach (var item in furniture) if (item != null && item.IsAnchored) anchored++;
            var points = level == 1 ? 400 : level == 2 ? 500 : level == 3 ? 600 : level == 4 ?
                (equipment.Contains("lightning_rod_system") ? 800 : 400) : 1000;
            score += points;
            report = level == 1 ? "Deprem çantası: " + (equipment.Contains("deprem_cantasi") ? "hazır" : "eksik") +
                "\nÇivilenmiş eşya: " + anchored + "\nEn yüksek eğim: " + maxTilt.ToString("F1") + "°" :
                level == 2 ? "Çatı güçlendirmesi: " + (equipment.Contains("storm_brace") ? "kurulu" : "yok") +
                "\nEn yüksek eğim: " + maxTilt.ToString("F1") + "°\nEvin dengesi korundu." :
                level == 3 ? "Yangın kaynağı: " + fireCause + "\nSöndürülen alev: " + extinguishedFires +
                "  |  Yayılma: " + fireSpreads + "\nDuman dedektörü: " +
                (equipment.Contains("duman_dedektoru") ? "erken uyarı verdi" : "yok") :
                level == 4 ? "Bina sağlığı: %" + Mathf.RoundToInt(buildingHealth) +
                "  |  Darbe: " + lightningHits + "\nParatonerin engellediği: " + lightningBlocked +
                "\nGerilim koruması: " + (equipment.Contains("surge_protector") ? "kurulu" : "yok") :
                "Tahliye edilen su: " + Mathf.RoundToInt(evacuatedWater) + " L" +
                "  |  Biriken: " + Mathf.RoundToInt(water) + " L\nİçeri sızma: %" +
                Mathf.RoundToInt(indoorFlood) + "  |  Eşik: " +
                (equipment.Contains("sandbags") ? "kum torbalı" : "korumasız") +
                "\nOluk / drenaj / pompa: " +
                (equipment.Contains("gutter_system") ? "✓" : "–") + " / " +
                (equipment.Contains("drainage_channel") ? "✓" : "–") + " / " +
                (equipment.Contains("water_pump") ? "✓" : "–");
            report += "\n+" + points + " bonus puan";
            var (income, lostCount) = SettleFurnitureIncome();
            report += "\nKorunan eşya getirisi: +" + income + " altın  |  Kaybedilen: " + lostCount;
            SetMusic(planningMusic);
            SetState(RoundState.Report);
            if (sfx != null) sfx.Play("level");
        }

        private void Advance()
        {
            if (state != RoundState.Report) return;
            if (level == 5) { score += 1500 + budget * 3; SetState(RoundState.Victory); if (sfx != null) sfx.Play("victory"); return; }
            foreach (var item in furniture)
            {
                if (item == null) continue;
                item.ResetPlanningSlide();
                if (!item.IsAnchored) continue;
                item.SetAnchored(false);
                RefreshAnchorVisual(item);
            }
            score += 500 + budget;
            level++;
            budget += budgetBonusPerLevel;
            timer = 45f;
            maxTilt = 0f;
            houseAngle = angularVelocity = 0f;
            if (balancePivot != null) balancePivot.localRotation = Quaternion.identity;
            if (level == 3 && !growDownward)
            {
                SetUpperFloorVisible(true);
                RefreshFurniture();
            }
            baselineTorque = ComputeTorque();
            smoothedTorque = baselineTorque;
            LockLevelEntryBalance();
            categoryIndex = level == 2 ? 0 : level == 3 ? 3 : level == 4 ? 4 : 5;
            marketOpen = !growDownward;
            SetState(RoundState.Planning);
            // Her yeni bölümde ev aşağı doğru büyür: mevcut katların altına yeni kat eklenir.
            if (growDownward) GrowFloorBelow();
            if (sfx != null) sfx.Play("level");
        }

        private void Lose(string reason)
        {
            if (state == RoundState.GameOver || state == RoundState.Victory) return;
            if (missionMode) { CompleteMissionLevel(false, reason); return; }
            if (level == 1 && singleFloorLevel && state == RoundState.Disaster) { CompleteEarthquakeLevel(false, reason); return; }
            report = reason;
            marketOpen = false;
            SetMusic(planningMusic);
            SetState(RoundState.GameOver);
            if (sfx != null) sfx.Play("defeat");
        }

        private void Restart()
        {
            startImmediatelyOnReload = true;
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadCurrent();
            else UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        [Tooltip("Ana menü sahnesinin adı (SceneLoader ile yüklenir).")]
        public string menuScene = "Menu";

        private void ReturnToMainMenu()
        {
            startImmediatelyOnReload = false;
            Time.timeScale = 1f;
            if (SceneLoader.Instance != null) SceneLoader.Instance.Load(menuScene);
        }

        private void ToggleMenu()
        {
            if (state == RoundState.Report || state == RoundState.GameOver || state == RoundState.Victory) return;
            menuOpen = !menuOpen;
            SyncPause();
        }

        private void SyncPause()
        {
            var playing = (state == RoundState.Planning || state == RoundState.Disaster) && !menuOpen && !howToOpen && !briefingOpen && !eggTipOpen;
            Time.timeScale = playing ? 1f : 0f;
            if (music != null)
            {
                if (!playing || musicMuted) music.Pause();
                else music.UnPause();
            }
        }

        private void SetState(RoundState next)
        {
            if (next != RoundState.Planning && drag != null) drag.DropCarried(true);
            state = next;
            if (next != RoundState.Disaster && resident != null) resident.SetHiding(false);
            if (next != RoundState.Disaster && foamVisual != null) foamVisual.SetActive(false);
            if (next != RoundState.Disaster) PoseExtinguisher(false);
            if (drag != null) drag.canDrag = next == RoundState.Planning;
            if (GameManager.Instance != null)
                GameManager.Instance.SetPhase(next == RoundState.Planning ? GamePhase.Preparation :
                    next == RoundState.Disaster ? GamePhase.Disaster : next == RoundState.GameOver ? GamePhase.GameOver : GamePhase.Results);
            SyncPause();
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (drag != null) drag.ManipulationStarted -= ReleaseLevelEntryBalance;
            if (looseFurnitureMaterial != null) Destroy(looseFurnitureMaterial);
            if (mediumFurnitureMaterial != null) Destroy(mediumFurnitureMaterial);
            if (heavyFurnitureMaterial != null) Destroy(heavyFurnitureMaterial);
            if (uiTransparent != null)
            {
                if (Application.isPlaying) Destroy(uiTransparent);
                else DestroyImmediate(uiTransparent);
            }
            if (paperFont != null)
            {
                if (Application.isPlaying) Destroy(paperFont);
                else DestroyImmediate(paperFont);
            }
            ReleaseFireMaterials();
            L1_PaperGui.Release();
        }

        private void SetMusic(AudioClip clip)
        {
            if (music == null || music.clip == clip) return;
            music.clip = clip;
            if (clip != null) music.Play();
        }

        private Sprite Icon(string id)
        {
            if (spriteEntries == null) return null;
            foreach (var entry in spriteEntries) if (entry != null && entry.id == id) return entry.sprite;
            return null;
        }

        private void Purchase(ShopItem item)
        {
            if (state != RoundState.Planning || IsLocked(item)) return;
            if (budget < item.cost) { Warn("Yeterli altın yok."); return; }
            if (item.category != "Mobilya" && item.id != "sabitleme_aparati" && equipment.Contains(item.id)) { Warn("Bu ekipman zaten kurulu."); return; }
            if (item.category == "Mobilya" && !SpawnFurniture(item)) { Warn("Açık katlarda yeni eşya için yer yok. Bir eşyayı satabilirsin."); return; }
            if (item.category == "Mobilya") ReleaseLevelEntryBalance();
            budget -= item.cost;
            if (item.category == "Mobilya") RefreshFurniture();
            if (item.id == "sabitleme_aparati") anchors++;
            else if (item.category != "Mobilya")
            {
                equipment.Add(item.id);
                if (item.id == "deprem_cantasi") SpawnBag();
                else if (item.id == "yangin_tupu") PrepareExtinguisherModel();
                else ShowEquipmentImage(item.id);
            }
            Warn(item.title + " eklendi.");
            if (sfx != null) sfx.Play("purchase");
        }

        private bool SpawnFurniture(ShopItem item, float? requiredFloorTop = null, float? preferredX = null)
        {
            if (item.template == null) return SpawnSpriteFurniture(item, requiredFloorTop, preferredX);
            furnitureTemplates.TryGetValue(item.id, out var source);
            if (source == null) return SpawnSpriteFurniture(item, requiredFloorTop, preferredX);
            var parent = houseRoot.Find("Bağımsız 3D mobilyalar");
            if (parent == null) parent = houseRoot;
            var clone = Instantiate(source.gameObject, parent);
            clone.SetActive(true);
            clone.name = item.title + " (market)";
            var movable = clone.GetComponent<L1_MovableProp>();
            movable.displayName = item.title;
            movable.itemId = item.id + "-" + Time.frameCount;
            movable.SetAnchored(false);
            RefreshAnchorVisual(movable);
            var body = clone.GetComponent<Rigidbody>();
            body.mass = Mathf.Clamp(item.mass / 18f, 1f, 6f);
            movable.balanceMassKg = item.mass;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            var box = clone.GetComponent<BoxCollider>();
            var start = source.transform.localPosition;
            if (!FindSpawnPosition(box, requiredFloorTop, preferredX, out var floorTop, out var x))
            { clone.SetActive(false); Destroy(clone); return false; }
            var floorY = floorTop + box.size.y * 0.5f + 0.03f;
            clone.transform.localPosition = new Vector3(x, floorY, start.z);
            // Kopya, kaynağın Rigidbody konumunu taşır; fizik onu eski yere geri yazmasın.
            body.position = clone.transform.position;
            body.rotation = clone.transform.rotation;
            movable.BeginPlacementGrace(requiredFloorTop.HasValue ? 2f : 1.5f, !requiredFloorTop.HasValue);
            Physics.SyncTransforms();
            return true;
        }

        private bool SpawnSpriteFurniture(ShopItem item, float? requiredFloorTop = null, float? preferredX = null)
        {
            var sprite = Icon(item.id);
            if (sprite == null) return false;
            var size = item.id == "wardrobe" ? new Vector3(0.75f, 1.45f, 0.4f) :
                item.id == "dining_table" ? new Vector3(1.2f, 0.7f, 0.5f) :
                item.id == "bedside_table" ? new Vector3(0.48f, 0.72f, 0.4f) :
                item.id == "standing_lamp" ? new Vector3(0.4f, 1.3f, 0.35f) : new Vector3(0.65f, 0.85f, 0.55f);
            var parent = houseRoot.Find("Bağımsız 3D mobilyalar");
            if (parent == null) parent = houseRoot;
            var root = new GameObject(item.title + " (2D geçici)");
            root.transform.SetParent(parent, false);
            var box = root.AddComponent<BoxCollider>();
            box.size = size;
            if (!FindSpawnPosition(box, requiredFloorTop, preferredX, out var floorTop, out var x))
            { root.SetActive(false); Destroy(root); return false; }
            root.transform.localPosition = new Vector3(x, floorTop + size.y * 0.5f + 0.03f, 0.25f);
            var body = root.AddComponent<Rigidbody>();
            body.mass = Mathf.Clamp(item.mass / 18f, 1f, 6f);
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            var visual = new GameObject("Görsel");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, -0.35f);
            visual.transform.localScale = new Vector3(size.x / sprite.bounds.size.x, size.y / sprite.bounds.size.y, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = projectSpriteMaterial;
            var movable = root.AddComponent<L1_MovableProp>();
            movable.displayName = item.title;
            movable.itemId = item.id + "-" + Time.frameCount;
            movable.balanceMassKg = item.mass;
            movable.BeginPlacementGrace(requiredFloorTop.HasValue ? 2f : 1.5f, !requiredFloorTop.HasValue);
            Physics.SyncTransforms();
            return true;
        }

        private bool FindSpawnPosition(BoxCollider box, float? requiredFloorTop, float? preferredX,
            out float floorTop, out float x)
        {
            var tops = FloorTops();
            var first = requiredFloorTop.HasValue ? 0 : UnityEngine.Random.Range(0, tops.Length);
            var count = requiredFloorTop.HasValue ? 1 : tops.Length;
            for (var i = 0; i < count; i++)
            {
                var top = requiredFloorTop ?? tops[(first + i) % tops.Length];
                var centerY = top + box.size.y * 0.5f + 0.03f;
                if (!FindFreePosition(box, centerY, preferredX, out x)) continue;
                floorTop = top;
                return true;
            }
            floorTop = 0f;
            x = 0f;
            return false;
        }

        private bool FindFreePosition(BoxCollider box, float localY, float? preferredX, out float result)
        {
            // The source game spawns purchases near the center and alternates
            // sides. Edge-first spawning made a newly bought heavy item create
            // a large balance impulse before the player could reposition it.
            var center = preferredX ?? 0f;
            var tries = preferredX.HasValue
                ? new[] { center, center + 0.5f, center - 0.5f, center + 1f, center - 1f,
                    0f, 1.6f, -1.6f, 2.8f, -2.8f, 3.7f, -3.7f }
                : new[] { 0f, 0.4f, -0.4f, 1f, -1f, 1.6f, -1.6f, 2.2f, -2.2f, 2.8f, -2.8f, 3.4f, -3.4f, 3.9f, -3.9f };
            var rotation = box.transform.rotation;
            var scale = box.transform.lossyScale;
            var half = Vector3.Scale(box.size * 0.5f,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            var right = houseRoot.right;
            var width = Mathf.Abs(Vector3.Dot(right, box.transform.right)) * half.x +
                Mathf.Abs(Vector3.Dot(right, box.transform.up)) * half.y +
                Mathf.Abs(Vector3.Dot(right, box.transform.forward)) * half.z;
            foreach (var x in tries)
            {
                var candidate = houseRoot.TransformPoint(new Vector3(x, localY, 0.25f)) +
                    rotation * Vector3.Scale(box.center, scale);
                if (Mathf.Abs(x) + width > roomHalfWidth - 0.11f) continue;
                var blocked = false;
                foreach (var hit in Physics.OverlapBox(candidate, half * 0.98f, rotation, ~0, QueryTriggerInteraction.Ignore))
                    if (!hit.transform.IsChildOf(box.transform) && hit.GetComponentInParent<L1_MovableProp>() != null) { blocked = true; break; }
                if (blocked) continue;
                result = x;
                return true;
            }
            result = 0f;
            return false;
        }

        private void SpawnBag()
        {
            if (bagOnBack) { AttachBagToResident(); return; }
            var sprite = Icon("deprem_cantasi");
            if (sprite == null) return;
            var root = new GameObject("Deprem çantası — taşınabilir");
            root.transform.SetParent(houseRoot, false);
            root.transform.localPosition = new Vector3(2.75f, FloorY[0] + 0.29f, 0.25f);
            var box = root.AddComponent<BoxCollider>();
            box.size = new Vector3(0.48f, 0.52f, 0.34f);
            var body = root.AddComponent<Rigidbody>();
            body.mass = 1.5f;
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            var visual = new GameObject("Çanta görseli");
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0f, -0.24f);
            visual.transform.localScale = new Vector3(0.48f / sprite.bounds.size.x, 0.52f / sprite.bounds.size.y, 1f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = projectSpriteMaterial;
            var movable = root.AddComponent<L1_MovableProp>();
            movable.displayName = "Deprem çantası";
            movable.itemId = "deprem_cantasi";
            movable.balanceMassKg = 10f;
            RefreshFurniture();
        }

        private void ShowEquipmentImage(string id)
        {
            var sprite = Icon(id);
            if (sprite == null) sprite = Icon("sabitleme_aparati");
            if (sprite == null) return;
            var floor = id == "lightning_rod_system" || id == "gutter_system" ? 2 :
                id == "storm_brace" || id == "duman_dedektoru" ? 1 : 0;
            var x = id == "yangin_tupu" ? 2.85f : id == "duman_dedektoru" ? -0.2f : 2.65f;
            var y = id == "lightning_rod_system" ? FloorY[2] + 3.0f :
                id == "duman_dedektoru" ? FloorY[1] + 2.0f : FloorY[floor] + 0.38f;
            var go = new GameObject("Ekipman görseli — " + id);
            go.transform.SetParent(houseRoot, false);
            go.transform.localPosition = new Vector3(x, y, -0.70f);
            go.transform.localScale = new Vector3(0.4f / sprite.bounds.size.x, 0.48f / sprite.bounds.size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = projectSpriteMaterial;
        }

        private void ToggleAnchor()
        {
            if (drag == null || drag.Selected == null) { Warn("Önce sabitlemek istediğin eşyayı seç."); return; }
            var item = drag.Selected;
            if (drag.Carried == item) { Warn("Sabitlemeden önce eşyayı yere bırak."); return; }
            if (item.IsAnchored) { item.SetAnchored(false); RefreshAnchorVisual(item); anchors++; Warn(PlayerFacingName(item) + " serbest bırakıldı."); if (sfx != null) sfx.Play("confirm"); return; }
            if (missionMode) { }
            else if (anchors <= 0)
            {
                if (budget < 30) { Warn("Sabitleme aparatı için 30 altın gerekli."); return; }
                budget -= 30;
            }
            else anchors--;
            item.SetAnchored(true);
            RefreshAnchorVisual(item);
            Warn(PlayerFacingName(item) + " sabitlendi.");
            if (sfx != null) sfx.Play("secure");
        }

        private void RefreshAnchorVisual(L1_MovableProp item)
        {
            var visual = item.transform.Find("Çivileme L-demiri");
            if (!item.IsAnchored)
            {
                if (visual != null) Destroy(visual.gameObject);
                return;
            }
            var sprite = Icon("sabitleme_aparati");
            if (sprite == null || visual != null) return;
            var go = new GameObject("Çivileme L-demiri");
            go.transform.SetParent(item.transform, false);
            var box = item.GetComponent<BoxCollider>();
            go.transform.localPosition = new Vector3(-box.size.x * 0.5f + 0.06f, -box.size.y * 0.5f + 0.20f, -0.47f);
            go.transform.localScale = new Vector3(0.32f / sprite.bounds.size.x, 0.32f / sprite.bounds.size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = projectSpriteMaterial;
        }

        private void SellSelected()
        {
            if (drag == null || drag.Selected == null) { Warn("Satılacak eşyayı seç."); return; }
            if (drag.Carried != null) drag.DropCarried(true);
            var item = drag.Selected;
            ShopItem catalogItem = null;
            foreach (var entry in Catalog)
                if ((item.itemId != null && item.itemId.StartsWith(entry.id)) ||
                    (entry.template != null && item.displayName != null && item.displayName.Contains(entry.template)))
                { catalogItem = entry; break; }
            var refund = catalogItem == null ? 30 : Mathf.FloorToInt(catalogItem.cost * 0.7f);
            budget += refund;
            if (item.IsAnchored) anchors++;
            if (item.itemId == "deprem_cantasi") equipment.Remove("deprem_cantasi");
            furniture.Remove(item);
            ReleaseLevelEntryBalance();
            Destroy(item.gameObject);
            Warn("Eşya satıldı. +" + refund + " altın");
            if (sfx != null) sfx.Play("coin");
        }

        private void Warn(string message) { warning = message; warningUntil = 4.5f; }

    }
}
