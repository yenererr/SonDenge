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

        private sealed class FireSpot
        {
            public GameObject visual;
            public float health;
            public float spreadClock;
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
            new ShopItem("red_armchair", "Tekli koltuk", "Mobilya", 90, 1, 35, "StylArts — orta kat tekli koltuk"),
            new ShopItem("coffee_table", "Ahşap sehpa", "Mobilya", 60, 1, 20, "MIX — sehpa"),
            new ShopItem("bookshelf", "Kitaplık", "Mobilya", 180, 1, 85, "MIX — dokulu kitaplık"),
            new ShopItem("refrigerator", "Retro buzdolabı", "Mobilya", 220, 2, 90, "StylArts — buzdolabı"),
            new ShopItem("stove_counter", "Ocak ve fırın", "Mobilya", 190, 2, 75, "StylArts — ocak ve fırın"),
            new ShopItem("dining_table", "Yemek masası", "Mobilya", 130, 2, 50),
            new ShopItem("wooden_barrel", "Erzak fıçısı", "Mobilya", 80, 2, 40),
            new ShopItem("cozy_bed", "Çift kişilik yatak", "Mobilya", 175, 3, 70, "Realistic Bed 01 — yatak"),
            new ShopItem("wardrobe", "Gardırop", "Mobilya", 200, 3, 85),
            new ShopItem("bedside_table", "Komodin ve şamdan", "Mobilya", 50, 3, 20, "Komodin"),
            new ShopItem("standing_lamp", "Pirinç lambader", "Mobilya", 45, 1, 15),
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

        private readonly HashSet<string> equipment = new HashSet<string>();
        private readonly List<FireSpot> fires = new List<FireSpot>();
        private readonly List<L1_MovableProp> furniture = new List<L1_MovableProp>();
        private readonly List<HangingLamp> hangingLamps = new List<HangingLamp>();
        private PhysicsMaterial looseFurnitureMaterial, mediumFurnitureMaterial, heavyFurnitureMaterial;
        private GameObject foamVisual;
        private AudioSource music;
        private L1_ProjectSfx sfx;
        private RoundState state = RoundState.StartMenu;
        private int level = 1, budget = 750, score, anchors;
        private float timer = 45f, houseAngle, angularVelocity, baselineTorque, smoothedTorque;
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
            music.volume = 0.42f;
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
            RefreshFurniture();
            CacheHangingLamps();
            baselineTorque = ComputeTorque();
            smoothedTorque = baselineTorque;
            if (startImmediatelyOnReload)
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
                if (item.balanceMassKg <= 0f)
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
                var target = Mathf.Clamp(houseMotion + idle, -16f, 16f);
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
            if (roof != null) roof.localPosition = new Vector3(0f, visible ? 0f : -2.25f, 0f);
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
            timer = 45f;
            maxTilt = 0f;
            evacuatedWater = waterSensorCooldown = 0f;
            extinguishedFires = fireSpreads = lightningHits = lightningBlocked = 0;
            marketOpen = menuOpen = howToOpen = false;
            SetMusic(planningMusic);
            SetState(RoundState.Planning);
            if (sfx != null) sfx.Play("confirm");
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && state != RoundState.StartMenu && !howToOpen)
                ToggleMenu();
            if (state == RoundState.StartMenu || menuOpen || howToOpen || Time.timeScale <= 0f) return;
            if (warningUntil > 0f) warningUntil -= Time.deltaTime;
            lightningFlash = Mathf.MoveTowards(lightningFlash, 0f, Time.deltaTime * 2.5f);
            if (keyboard != null)
            {
                if (keyboard.iKey.wasPressedThisFrame) marketOpen = !marketOpen && state == RoundState.Planning;
                if (keyboard.spaceKey.wasPressedThisFrame && state == RoundState.Planning) StartDisaster();
                if (keyboard.fKey.wasPressedThisFrame && state == RoundState.Planning) ToggleAnchor();
                if (keyboard.rKey.wasPressedThisFrame && (state == RoundState.GameOver || state == RoundState.Victory)) Restart();
            }

            if (state == RoundState.Planning)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f) StartDisaster(true);
            }
            else if (state == RoundState.Disaster)
            {
                timer -= Time.deltaTime;
                TickDisaster(Time.deltaTime, keyboard);
                if (state == RoundState.Disaster && timer <= 0f)
                {
                    if (level == 3 && fires.Count > 0) Lose("Yangın zamanında söndürülemedi.");
                    else CompleteLevel();
                }
            }
        }

        private void FixedUpdate()
        {
            if (balancePivot == null || houseRoot == null || state == RoundState.StartMenu ||
                state == RoundState.Report || state == RoundState.GameOver || state == RoundState.Victory || menuOpen || howToOpen) return;
            var disturbance = 0f;
            if (state == RoundState.Disaster && level == 1)
                disturbance = Mathf.Sin(Time.time * 5.2f) * 0.015f + Mathf.Sin(Time.time * 8.3f) * 0.008f;
            else if (state == RoundState.Disaster && level == 2)
                disturbance = (equipment.Contains("storm_brace") ? 0.012f : 0.035f) * (1f + Mathf.Sin(Time.time * 1.3f));

            foreach (var item in furniture)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.IsAnchored || item.Body == null || item.Body.isKinematic) continue;
                // Gravity already pulls props down the rotated floor. A small,
                // continuous assist overcomes contact friction without a sudden threshold.
                var slope = -Mathf.Sin(houseAngle) * 2.5f;
                var shake = state == RoundState.Disaster && level == 1 ? Mathf.Sin(Time.time * 8f) * 1.4f : 0f;
                var wind = state == RoundState.Disaster && level == 2 ? (equipment.Contains("storm_brace") ? 0.8f : 2.5f) : 0f;
                item.Body.AddForce(houseRoot.right * (slope + shake + wind), ForceMode.Acceleration);
                var along = Vector3.Dot(item.Body.linearVelocity, houseRoot.right);
                var excessSpeed = Mathf.Max(0f, Mathf.Abs(along) - 1.1f);
                if (excessSpeed > 0f)
                    item.Body.AddForce(-houseRoot.right * (Mathf.Sign(along) * excessSpeed * 4f),
                        ForceMode.Acceleration);
            }

            // Positive weight on the right lowers the right side: clockwise / negative Z.
            // Filter the moving furniture's torque and ignore tiny settling jitter.
            smoothedTorque = Mathf.Lerp(smoothedTorque, ComputeTorque(),
                1f - Mathf.Exp(-Time.fixedDeltaTime / 0.95f));
            var torqueDelta = smoothedTorque - baselineTorque;
            // A soft dead zone removes tiny settling jitter while keeping the
            // response continuous as a resident or prop crosses the center.
            torqueDelta = Mathf.Sign(torqueDelta) *
                (Mathf.Sqrt(torqueDelta * torqueDelta + 25f) - 5f);
            // Use the same limit before and during a disaster; switching states
            // must not release a previously clamped target in one step.
            var target = Mathf.Clamp(-torqueDelta * 0.0016f + disturbance, -0.52f, 0.52f);
            // The source game keeps the house nearly level from the fire stage
            // onward so those rounds are decided by their own hazards. Retain a
            // small visible response for the physical 3D house.
            if (level >= 3) target = Mathf.Clamp(target * 0.16f, -0.065f, 0.065f);
            houseAngle = Mathf.SmoothDamp(houseAngle, target, ref angularVelocity,
                1.55f, 0.15f, Time.fixedDeltaTime);
            balancePivot.localRotation = Quaternion.Euler(0f, 0f, houseAngle * Mathf.Rad2Deg);
            maxTilt = Mathf.Max(maxTilt, Mathf.Abs(houseAngle * Mathf.Rad2Deg));
            if (state == RoundState.Disaster && level <= 2 && Mathf.Abs(houseAngle) > 26f * Mathf.Deg2Rad)
                Lose("Ev denge sınırını aşıp devrildi.");
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
                var edgeWeight = 1f - Mathf.SmoothStep(3.25f, 4.65f, Mathf.Abs(local.x));
                var heightWeight = Mathf.SmoothStep(-2.8f, -1.8f, local.y);
                torque += item.Mass * local.x * edgeWeight * heightWeight;
            }
            if (drag != null && drag.Carried != null && drag.Carried.Body != null)
            {
                // A carried item no longer presses on the floor, but its load
                // still acts at the resident's hand position as they walk.
                var carriedMass = Mathf.Max(10f, drag.Carried.Mass * 0.75f);
                torque += carriedMass * drag.CarriedLocalX;
            }
            if (resident != null)
                torque += 8f * houseRoot.InverseTransformPoint(resident.transform.position).x;
            return torque;
        }

        private void TickDisaster(float dt, Keyboard keyboard)
        {
            if (level == 3)
            {
                var spraying = equipment.Contains("yangin_tupu") &&
                    ((keyboard != null && (keyboard.eKey.isPressed || keyboard.enterKey.isPressed)) || L1_TouchControls.ActionHeld);
                spraying &= resident != null && !resident.IsClimbing;
                ShowFoam(spraying);
                for (var i = fires.Count - 1; i >= 0; i--)
                {
                    var fire = fires[i];
                    fire.spreadClock += dt;
                    if (spraying && resident != null)
                    {
                        var player = houseRoot.InverseTransformPoint(resident.transform.position);
                        var playerFloor = Mathf.Clamp(Mathf.RoundToInt(
                            (player.y - resident.footOffset - FloorY[0]) / 2.25f), 0, 2);
                        var delta = fire.x - player.x;
                        if (fire.floor == playerFloor && Mathf.Abs(delta) < 1.75f &&
                            delta * resident.Facing >= -0.35f)
                            fire.health -= dt * 75f;
                    }
                    if (fire.health <= 0f)
                    {
                        Destroy(fire.visual);
                        fires.RemoveAt(i);
                        extinguishedFires++;
                        score += 300;
                        if (sfx != null) sfx.Play("level");
                    }
                    else if (fire.spreadClock > 16f && fires.Count < 4 && fire.health > 60f)
                    {
                        fire.spreadClock = 0f;
                        SpawnFire(UnityEngine.Random.Range(0, 3), UnityEngine.Random.Range(-1.2f, 1.2f), 75f);
                        fireSpreads++;
                    }
                }
                if (fires.Count == 0) CompleteLevel();
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
                        buildingHealth -= equipment.Contains("surge_protector") ? 25f : 37f;
                        if (sfx != null) sfx.Play("strike");
                    }
                    else
                    {
                        lightningBlocked++;
                        if (sfx != null) sfx.Play("shield");
                    }
                    if (buildingHealth <= 0f) Lose("Yıldırım darbeleri evi yıktı.");
                }
            }
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
            if (state != RoundState.Planning) return;
            if (level == 1 && !equipment.Contains("deprem_cantasi") && !forced) { Warn("Deprem çantası olmadan afeti başlatamazsın. Marketten edin."); return; }
            if (level == 3 && !equipment.Contains("yangin_tupu") && !forced) { Warn("Yangın tüpü olmadan yangına müdahale edemezsin. Marketten edin."); return; }
            if (level <= 2 && Mathf.Abs(houseAngle) > 18f * Mathf.Deg2Rad && !forced) { Warn("Ev çok eğik. Önce eşyaları dengele."); return; }
            marketOpen = false;
            timer = level == 1 ? 10f : level == 2 ? 12f : level == 3 ? 30f : 25f;
            if (level == 3)
            {
                extinguishedFires = fireSpreads = 0;
                var source = UnityEngine.Random.Range(0, 4);
                var floor = source == 1 ? 1 : source == 2 ? 2 : 0;
                var x = source == 0 ? -0.2f : source == 1 ? 1.1f : source == 2 ? -0.7f : 0.8f;
                fireCause = source == 0 ? "Mutfakta aşırı ısınan ocak" : source == 1 ?
                    "Oturma odasında aşırı yüklü priz" : source == 2 ?
                    "Yatak odasında arızalı ısıtıcı" : "Ocakta unutulan kızgın tava";
                SpawnFire(floor, x, 100f);
                SpawnFire(floor == 1 ? 0 : 1, x > 0f ? -0.8f : 0.8f, 85f);
                if (equipment.Contains("duman_dedektoru")) Warn("Duman dedektörü: yangın erken algılandı!");
            }
            if (level == 4) { buildingHealth = 100f; lightningClock = 2.5f; lightningHits = lightningBlocked = 0; }
            if (level == 5) { water = indoorFlood = evacuatedWater = waterSensorCooldown = 0f; }
            SetMusic(disasterMusic);
            SetState(RoundState.Disaster);
            if (sfx != null) sfx.Play(level == 3 ? "strike" : "confirm");
        }

        private void CompleteLevel()
        {
            if (state != RoundState.Disaster) return;
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
            SetMusic(planningMusic);
            SetState(RoundState.Report);
            if (sfx != null) sfx.Play("level");
        }

        private void Advance()
        {
            if (state != RoundState.Report) return;
            if (level == 5) { score += 1500 + budget * 3; SetState(RoundState.Victory); if (sfx != null) sfx.Play("victory"); return; }
            score += 500 + budget;
            level++;
            budget += 450;
            timer = 45f;
            maxTilt = 0f;
            houseAngle = angularVelocity = 0f;
            if (balancePivot != null) balancePivot.localRotation = Quaternion.identity;
            if (level == 3)
            {
                SetUpperFloorVisible(true);
                RefreshFurniture();
            }
            baselineTorque = ComputeTorque();
            smoothedTorque = baselineTorque;
            categoryIndex = level == 2 ? 0 : level == 3 ? 3 : level == 4 ? 4 : 5;
            marketOpen = true;
            SetState(RoundState.Planning);
            if (sfx != null) sfx.Play("level");
        }

        private void Lose(string reason)
        {
            if (state == RoundState.GameOver || state == RoundState.Victory) return;
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
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void ReturnToMainMenu()
        {
            startImmediatelyOnReload = false;
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void ToggleMenu()
        {
            if (state == RoundState.Report || state == RoundState.GameOver || state == RoundState.Victory) return;
            menuOpen = !menuOpen;
            SyncPause();
        }

        private void SyncPause()
        {
            var playing = (state == RoundState.Planning || state == RoundState.Disaster) && !menuOpen && !howToOpen;
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
            if (next != RoundState.Disaster && foamVisual != null) foamVisual.SetActive(false);
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

        private void SpawnFire(int floor, float x, float health)
        {
            var sprite = Icon("fire_flame");
            var visual = new GameObject("Yangın alevi — " + (floor + 1) + ". kat");
            visual.transform.SetParent(houseRoot, false);
            visual.transform.localPosition = new Vector3(x, FloorY[floor] + 0.45f, -0.66f);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = projectSpriteMaterial;
            if (sprite != null) visual.transform.localScale = new Vector3(0.65f / sprite.bounds.size.x, 0.9f / sprite.bounds.size.y, 1f);
            fires.Add(new FireSpot { visual = visual, health = health, floor = floor, x = x });
        }

        private void ShowFoam(bool spraying)
        {
            if (!spraying || resident == null)
            {
                if (foamVisual != null) foamVisual.SetActive(false);
                return;
            }
            if (foamVisual == null)
            {
                var sprite = Icon("foam_cloud");
                if (sprite == null) return;
                foamVisual = new GameObject("Yangın tüpü köpüğü");
                foamVisual.transform.SetParent(houseRoot, false);
                foamVisual.transform.localScale = new Vector3(0.7f / sprite.bounds.size.x, 0.35f / sprite.bounds.size.y, 1f);
                var renderer = foamVisual.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = projectSpriteMaterial;
            }
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            foamVisual.transform.localPosition = new Vector3(player.x + resident.Facing * 0.48f,
                player.y + 0.22f, -1.5f);
            foamVisual.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 19f) * 12f);
            foamVisual.SetActive(true);
        }

        private void Purchase(ShopItem item)
        {
            if (state != RoundState.Planning || level < item.unlockLevel) return;
            if (budget < item.cost) { Warn("Yeterli altın yok."); return; }
            if (item.category != "Mobilya" && item.id != "sabitleme_aparati" && equipment.Contains(item.id)) { Warn("Bu ekipman zaten kurulu."); return; }
            if (item.category == "Mobilya" && !SpawnFurniture(item)) { Warn("Bu katta yeni eşya için yer yok. Bir eşyayı satabilirsin."); return; }
            budget -= item.cost;
            if (item.category == "Mobilya") RefreshFurniture();
            if (item.id == "sabitleme_aparati") anchors++;
            else if (item.category != "Mobilya")
            {
                equipment.Add(item.id);
                if (item.id == "deprem_cantasi") SpawnBag();
                else ShowEquipmentImage(item.id);
            }
            Warn(item.title + " eklendi.");
            if (sfx != null) sfx.Play("purchase");
        }

        private bool SpawnFurniture(ShopItem item)
        {
            if (item.template == null) return SpawnSpriteFurniture(item);
            L1_MovableProp source = null;
            foreach (var prop in furniture)
                if (prop.displayName != null && prop.displayName.Contains(item.template)) { source = prop; break; }
            if (source == null) return SpawnSpriteFurniture(item);
            var clone = Instantiate(source.gameObject, source.transform.parent);
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
            var floor = item.unlockLevel <= 1 ? 1 : item.unlockLevel == 2 ? 0 : 2;
            var box = clone.GetComponent<BoxCollider>();
            var floorY = FloorY[floor] + box.size.y * 0.5f + 0.03f;
            var start = source.transform.localPosition;
            if (!FindFreePosition(box, floorY, out var x))
            { Destroy(clone); return false; }
            clone.transform.localPosition = new Vector3(x, floorY, start.z);
            return true;
        }

        private bool SpawnSpriteFurniture(ShopItem item)
        {
            var sprite = Icon(item.id);
            if (sprite == null) return false;
            var floor = item.unlockLevel <= 1 ? 1 : item.unlockLevel == 2 ? 0 : 2;
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
            if (!FindFreePosition(box, FloorY[floor] + size.y * 0.5f + 0.03f, out var x))
            { Destroy(root); return false; }
            root.transform.localPosition = new Vector3(x, FloorY[floor] + size.y * 0.5f + 0.03f, 0.25f);
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
            return true;
        }

        private bool FindFreePosition(BoxCollider box, float localY, out float result)
        {
            // The source game spawns purchases near the center and alternates
            // sides. Edge-first spawning made a newly bought heavy item create
            // a large balance impulse before the player could reposition it.
            var tries = new[] { 0f, 0.4f, -0.4f, 1f, -1f, 1.6f, -1.6f, 2.2f, -2.2f, 2.8f, -2.8f };
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
                if (Mathf.Abs(x) + width > 3.22f) continue;
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
            if (item.IsAnchored) { item.SetAnchored(false); RefreshAnchorVisual(item); anchors++; Warn(item.displayName + " serbest bırakıldı."); if (sfx != null) sfx.Play("confirm"); return; }
            if (anchors <= 0)
            {
                if (budget < 30) { Warn("Sabitleme aparatı için 30 altın gerekli."); return; }
                budget -= 30;
            }
            else anchors--;
            item.SetAnchored(true);
            RefreshAnchorVisual(item);
            Warn(item.displayName + " sabitlendi.");
            if (sfx != null) sfx.Play("confirm");
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
            Destroy(item.gameObject);
            Warn("Eşya satıldı. +" + refund + " altın");
            if (sfx != null) sfx.Play("coin");
        }

        private void Warn(string message) { warning = message; warningUntil = 4.5f; }

    }
}
