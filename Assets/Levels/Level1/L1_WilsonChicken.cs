using System.Collections.Generic;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>Wilson wanders over the active floors, uses the resident's ladders and lays collectible eggs.</summary>
    public sealed class L1_WilsonChicken : MonoBehaviour
    {
        private enum Activity { Walking, Feeding, ApproachingLadder, Climbing }

        private const float WalkSpeed = 0.82f;
        private const float ClimbSpeed = 0.7f;
        private const float EggMassKg = 5f;
        private const int MaxEggs = 8;

        private readonly List<L1_WilsonEgg> eggs = new List<L1_WilsonEgg>();
        private L1_ProjectGameController game;
        private L1_ResidentController resident;
        private Transform house;
        private Transform visual;
        private GameObject eggModel;
        private Material chickenMaterial, shellMaterial, goldMaterial, feedMaterial;
        private Activity activity;
        private L1_ClimbRoute ladder;
        private Vector3 climbStart, climbEnd;
        private float floorTop, destinationX, pauseUntil, nextEggAt, climbProgress;
        private int facing = 1;

        public void Initialize(L1_ProjectGameController controller, Transform houseRoot,
            L1_ResidentController houseResident)
        {
            game = controller;
            house = houseRoot;
            resident = houseResident;
            var chickenModel = Resources.Load<GameObject>("Wilson/low_poly_chicken");
            eggModel = Resources.Load<GameObject>("Wilson/egg");
            if (chickenModel == null || eggModel == null)
            {
                Debug.LogError("Wilson için tavuk veya yumurta modeli bulunamadı (Resources/Wilson).", this);
                enabled = false;
                return;
            }

            chickenMaterial = MakeMaterial(new Color(1f, 1f, 1f),
                Resources.Load<Texture2D>("Wilson/low_poly_chicken"));
            shellMaterial = MakeMaterial(new Color(0.96f, 0.88f, 0.68f));
            goldMaterial = MakeMaterial(new Color(1f, 0.69f, 0.08f), null, true);
            feedMaterial = MakeMaterial(new Color(0.55f, 0.36f, 0.15f));

            visual = new GameObject("Wilson 3D model").transform;
            visual.SetParent(transform, false);
            var model = Instantiate(chickenModel, visual);
            model.name = "Low Poly Chicken";
            FitModel(model.transform, visual, 0.60f, chickenMaterial);
            floorTop = controller.FloorTops()[0];
            transform.localPosition = new Vector3(1.2f, floorTop, -0.55f);
            destinationX = -1.6f;
            nextEggAt = Time.time + Random.Range(15f, 24f);
        }

        private void Update()
        {
            if (game == null || house == null || !(game.IsPlanning || game.IsDisaster) ||
                game.IsGrowing || game.IsPausedMenu || game.HasModal || Time.timeScale <= 0f) return;

            if (Time.time >= nextEggAt && activity != Activity.Climbing && eggs.Count < MaxEggs)
            {
                LayEgg();
                nextEggAt = Time.time + Random.Range(18f, 30f);
            }
            WalkOrFeed(Time.deltaTime);
            CheckEggs();
        }

        private void WalkOrFeed(float dt)
        {
            if (activity == Activity.Climbing)
            {
                var distance = Vector3.Distance(climbStart, climbEnd);
                climbProgress = Mathf.MoveTowards(climbProgress, 1f,
                    dt * ClimbSpeed / Mathf.Max(0.01f, distance));
                transform.localPosition = Vector3.Lerp(climbStart, climbEnd, climbProgress);
                if (climbProgress >= 1f)
                {
                    floorTop = NearestFloorTop(transform.localPosition.y);
                    transform.localPosition = new Vector3(transform.localPosition.x, floorTop, -0.55f);
                    if (ladder != null && (resident == null || !resident.IsClimbing)) ladder.SetOpen(false);
                    ladder = null;
                    BeginFeeding();
                }
                Animate(false);
                return;
            }

            if (activity == Activity.Feeding)
            {
                Animate(true);
                if (Time.time >= pauseUntil) ChooseDestination();
                return;
            }

            var current = transform.localPosition;
            var distanceX = destinationX - current.x;
            if (Mathf.Abs(distanceX) < 0.08f)
            {
                current.x = destinationX;
                transform.localPosition = current;
                if (activity == Activity.ApproachingLadder) BeginClimb();
                else BeginFeeding();
            }
            else
            {
                facing = distanceX > 0f ? 1 : -1;
                current.x = Mathf.MoveTowards(current.x, destinationX, dt * WalkSpeed);
                current.y = floorTop;
                transform.localPosition = current;
                Animate(false);
            }
        }

        private void ChooseDestination()
        {
            var floors = game.FloorTops();
            var currentIndex = NearestFloorIndex(floorTop, floors);
            if (floors.Length > 1 && Random.value < 0.36f && TryChooseLadder(currentIndex, floors))
                return;
            activity = Activity.Walking;
            var floorCenter = game.FloorCenterX(currentIndex);
            destinationX = Random.Range(floorCenter - 2.35f, floorCenter + 2.35f);
        }

        private bool TryChooseLadder(int floorIndex, float[] floors)
        {
            if (resident == null || resident.ladders == null) return false;
            var direction = floorIndex == 0 ? 1 : floorIndex == floors.Length - 1 ? -1 :
                (Random.value < 0.5f ? -1 : 1);
            var nextIndex = floorIndex + direction;
            foreach (var route in resident.ladders)
            {
                if (route == null || !route.gameObject.activeInHierarchy ||
                    route.bottom == null || route.landing == null) continue;
                var bottom = house.InverseTransformPoint(route.bottom.position);
                var landing = house.InverseTransformPoint(route.landing.position);
                var lowerIndex = NearestFloorIndex(bottom.y, floors);
                var upperIndex = NearestFloorIndex(landing.y, floors);
                if (direction > 0 && lowerIndex != floorIndex ||
                    direction < 0 && upperIndex != floorIndex ||
                    Mathf.Abs(upperIndex - lowerIndex) != 1 || nextIndex != (direction > 0 ? upperIndex : lowerIndex))
                    continue;
                ladder = route;
                activity = Activity.ApproachingLadder;
                destinationX = direction > 0 ? bottom.x : landing.x;
                destinationX = Mathf.Clamp(destinationX, -game.roomHalfWidth + 0.4f,
                    game.roomHalfWidth - 0.4f);
                return true;
            }
            return false;
        }

        private void BeginClimb()
        {
            if (ladder == null || !ladder.gameObject.activeInHierarchy)
            {
                BeginFeeding();
                return;
            }
            var floors = game.FloorTops();
            var bottom = house.InverseTransformPoint(ladder.bottom.position);
            var landing = house.InverseTransformPoint(ladder.landing.position);
            var lowerTop = floors[NearestFloorIndex(bottom.y, floors)];
            var upperTop = floors[NearestFloorIndex(landing.y, floors)];
            var goingUp = Mathf.Abs(floorTop - lowerTop) < Mathf.Abs(floorTop - upperTop);
            climbStart = goingUp ? new Vector3(bottom.x, lowerTop, -0.55f) :
                new Vector3(landing.x, upperTop, -0.55f);
            climbEnd = goingUp ? new Vector3(landing.x, upperTop, -0.55f) :
                new Vector3(bottom.x, lowerTop, -0.55f);
            transform.localPosition = climbStart;
            climbProgress = 0f;
            ladder.SetOpen(true);
            activity = Activity.Climbing;
        }

        private void BeginFeeding()
        {
            activity = Activity.Feeding;
            pauseUntil = Time.time + Random.Range(1.4f, 3f);
            for (var i = 0; i < 3; i++)
            {
                var grain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                grain.name = "Wilson yemi";
                grain.transform.SetParent(house, false);
                grain.transform.localPosition = transform.localPosition +
                    new Vector3(facing * (0.22f + i * 0.09f), 0.025f, 0.08f);
                grain.transform.localScale = new Vector3(0.045f, 0.025f, 0.045f);
                grain.GetComponent<Renderer>().sharedMaterial = feedMaterial;
                Destroy(grain.GetComponent<Collider>());
                Destroy(grain, 3.2f);
            }
        }

        private void Animate(bool feeding)
        {
            if (visual == null) return;
            var bob = feeding ? Mathf.Abs(Mathf.Sin(Time.time * 12f)) * 0.07f :
                Mathf.Abs(Mathf.Sin(Time.time * 9f)) * 0.025f;
            visual.localPosition = new Vector3(0f, -bob, 0f);
            visual.localRotation = Quaternion.Euler(feeding ? 18f : 0f, facing > 0 ? 90f : -90f, 0f);
        }

        private void LayEgg()
        {
            var golden = Random.value < 0.08f;
            var root = new GameObject(golden ? "Wilson altın yumurtası" : "Wilson yumurtası");
            root.transform.SetParent(house, false);
            root.transform.localPosition = transform.localPosition + new Vector3(-facing * 0.18f, 0.34f, 0f);
            var shell = Instantiate(eggModel, root.transform);
            shell.name = golden ? "Işıldayan altın yumurta" : "Normal yumurta";
            FitModel(shell.transform, root.transform, 0.28f, golden ? goldMaterial : shellMaterial);
            var collider = root.AddComponent<SphereCollider>();
            collider.radius = 0.14f;
            collider.center = new Vector3(0f, 0.14f, 0f);
            var body = root.AddComponent<Rigidbody>();
            body.mass = EggMassKg;
            body.constraints = RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            var egg = root.AddComponent<L1_WilsonEgg>();
            egg.Golden = golden;
            eggs.Add(egg);
            if (golden)
            {
                var light = root.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.73f, 0.19f);
                light.intensity = 0.75f;
                light.range = 1.3f;
            }
        }

        private void CheckEggs()
        {
            var player = resident != null ? house.InverseTransformPoint(resident.transform.position) : Vector3.zero;
            for (var i = eggs.Count - 1; i >= 0; i--)
            {
                var egg = eggs[i];
                if (egg == null) { eggs.RemoveAt(i); continue; }
                var position = house.InverseTransformPoint(egg.Body.position);
                if (position.y < game.lowestFloorTop - 1.45f ||
                    Mathf.Abs(position.x) > game.roomHalfWidth + 1.47f)
                {
                    eggs.RemoveAt(i);
                    Destroy(egg.gameObject);
                    continue;
                }
                if (resident == null || Mathf.Abs(player.x - position.x) > 0.43f ||
                    Mathf.Abs(player.y - position.y) > 0.62f) continue;
                if (egg.Golden) GoldBurst(egg.transform.position);
                game.WilsonEggCollected(egg.Golden);
                eggs.RemoveAt(i);
                Destroy(egg.gameObject);
            }
        }

        public float EggTorque()
        {
            if (house == null) return 0f;
            var total = 0f;
            foreach (var egg in eggs)
            {
                if (egg == null || egg.Body == null) continue;
                var local = house.InverseTransformPoint(egg.Body.position);
                if (local.y < game.lowestFloorTop - 1.45f ||
                    Mathf.Abs(local.x) > game.roomHalfWidth + 1.47f) continue;
                total += EggMassKg * local.x;
            }
            return total;
        }

        private void GoldBurst(Vector3 position)
        {
            var origin = house.InverseTransformPoint(position);
            for (var i = 0; i < 12; i++)
            {
                var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = "Altın yumurta ışıltısı";
                spark.transform.SetParent(house, false);
                spark.transform.localPosition = origin;
                spark.GetComponent<Renderer>().sharedMaterial = goldMaterial;
                Destroy(spark.GetComponent<Collider>());
                var angle = i * Mathf.PI * 2f / 12f;
                spark.AddComponent<L1_WilsonSparkle>().Launch(
                    new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * Random.Range(0.65f, 1.1f));
            }
        }

        private float NearestFloorTop(float y)
        {
            var floors = game.FloorTops();
            return floors[NearestFloorIndex(y, floors)];
        }

        private static int NearestFloorIndex(float y, float[] floors)
        {
            var best = 0;
            for (var i = 1; i < floors.Length; i++)
                if (Mathf.Abs(y - floors[i]) < Mathf.Abs(y - floors[best])) best = i;
            return best;
        }

        private static Material MakeMaterial(Color color, Texture texture = null, bool glowing = false)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var material = new Material(shader) { color = color };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Cull")) material.SetFloat("_Cull", 0f);
            if (texture != null)
            {
                if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
                if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            }
            if (glowing)
            {
                if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0.75f);
                if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.7f);
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", new Color(1.7f, 0.95f, 0.12f));
            }
            return material;
        }

        private static void FitModel(Transform model, Transform host, float desiredHeight, Material material)
        {
            var renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            model.localScale *= desiredHeight / Mathf.Max(0.01f, bounds.size.y);
            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            var bottom = host.InverseTransformPoint(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
            model.localPosition -= bottom;
            foreach (var renderer in renderers) renderer.sharedMaterial = material;
            foreach (var collider in model.GetComponentsInChildren<Collider>(true)) Destroy(collider);
        }

        private void OnDestroy()
        {
            if (chickenMaterial != null) Destroy(chickenMaterial);
            if (shellMaterial != null) Destroy(shellMaterial);
            if (goldMaterial != null) Destroy(goldMaterial);
            if (feedMaterial != null) Destroy(feedMaterial);
        }
    }

    public sealed class L1_WilsonEgg : MonoBehaviour
    {
        public bool Golden { get; set; }
        public Rigidbody Body { get; private set; }
        private void Awake() => Body = GetComponent<Rigidbody>();
    }

    public sealed class L1_WilsonSparkle : MonoBehaviour
    {
        private Vector3 origin, velocity;
        private float age;
        public void Launch(Vector3 direction)
        {
            origin = transform.localPosition;
            velocity = direction;
            transform.localScale = Vector3.one * 0.07f;
        }
        private void Update()
        {
            age += Time.deltaTime;
            transform.localPosition = origin + velocity * age + Vector3.down * (age * age * 0.4f);
            transform.localScale = Vector3.one * (0.07f * Mathf.Max(0f, 1f - age / 0.85f));
            if (age >= 0.85f) Destroy(gameObject);
        }
    }
}
