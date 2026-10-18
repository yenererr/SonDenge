using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jam.Levels.Level1
{
    public sealed partial class L1_ProjectGameController
    {
        [Header("Yangın efektleri")]
        public GameObject fireVfxPrefab;
        public GameObject smokeVfxPrefab;
        public GameObject extinguisherPrefab;

        private readonly Dictionary<int, HashSet<int>> floorFireInventory = new Dictionary<int, HashSet<int>>();
        private readonly HashSet<int> burnedFurniture = new HashSet<int>();
        private readonly HashSet<int> collapsedFloors = new HashSet<int>();
        private readonly List<Material> fireRuntimeMaterials = new List<Material>();
        private GameObject carriedExtinguisher;
        private bool floorCollapsePending;

        private int FireFloorOf(Vector3 localPosition)
        {
            return Mathf.Clamp(Mathf.RoundToInt((localPosition.y - lowestFloorTop) / floorHeight), 0, FloorCount - 1);
        }

        private void BeginFireDisaster()
        {
            floorFireInventory.Clear();
            burnedFurniture.Clear();
            collapsedFloors.Clear();
            floorCollapsePending = false;
            for (var i = 0; i < FloorCount; i++) floorFireInventory[i] = new HashSet<int>();
            foreach (var item in furniture)
            {
                if (item == null || !item.gameObject.activeInHierarchy || item.Body == null) continue;
                var local = houseRoot.InverseTransformPoint(item.Body.position);
                if (Mathf.Abs(local.y - FloorTops()[FireFloorOf(local)] ) > 2.1f) continue;
                floorFireInventory[FireFloorOf(local)].Add(item.GetInstanceID());
            }
            PrepareExtinguisherModel();
        }

        private void PrepareExtinguisherModel()
        {
            if (!equipment.Contains("yangin_tupu") || resident == null || carriedExtinguisher != null || extinguisherPrefab == null) return;
            var parent = resident.visual != null ? resident.visual : resident.transform;
            carriedExtinguisher = Instantiate(extinguisherPrefab, parent);
            carriedExtinguisher.name = "Taşınan yangın tüpü";
            foreach (var collider in carriedExtinguisher.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            foreach (var body in carriedExtinguisher.GetComponentsInChildren<Rigidbody>(true)) Destroy(body);
            var renderer = carriedExtinguisher.GetComponentInChildren<Renderer>();
            var height = renderer != null ? renderer.bounds.size.y : 1f;
            carriedExtinguisher.transform.localScale = Vector3.one * (0.58f / Mathf.Max(0.1f, height));
            ConvertExtinguisherMaterials(carriedExtinguisher);
            PoseExtinguisher(false);
        }

        private void PoseExtinguisher(bool spraying)
        {
            if (carriedExtinguisher == null || resident == null) return;
            var facing = resident.Facing;
            carriedExtinguisher.transform.localPosition = spraying
                ? new Vector3(facing * 0.35f, -0.03f, -0.35f)
                : new Vector3(-facing * 0.22f, -0.12f, 0.20f);
            carriedExtinguisher.transform.localRotation = Quaternion.Euler(0f, 0f, spraying ? -facing * 68f : facing * 13f);
        }

        private void ConvertExtinguisherMaterials(GameObject model)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) return;
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var original = renderer.sharedMaterial;
                if (original == null || original.shader == shader) continue;
                var converted = new Material(shader);
                var albedo = original.HasProperty("_MainTex") ? original.GetTexture("_MainTex") : null;
                if (albedo != null) converted.SetTexture("_BaseMap", albedo);
                var color = original.HasProperty("_Color") ? original.GetColor("_Color") : Color.white;
                converted.SetColor("_BaseColor", color);
                if (original.HasProperty("_BumpMap")) converted.SetTexture("_BumpMap", original.GetTexture("_BumpMap"));
                renderer.material = converted;
                fireRuntimeMaterials.Add(converted);
            }
        }

        private bool IsBurning(L1_MovableProp item)
        {
            foreach (var fire in fires) if (fire.target == item) return true;
            return false;
        }

        private L1_MovableProp FindFireTarget(int floor, float x, float maxDistance = float.PositiveInfinity)
        {
            L1_MovableProp best = null;
            var bestDistance = maxDistance;
            foreach (var item in furniture)
            {
                if (item == null || item.Body == null || !item.gameObject.activeInHierarchy ||
                    burnedFurniture.Contains(item.GetInstanceID()) || IsBurning(item)) continue;
                var local = houseRoot.InverseTransformPoint(item.Body.position);
                var candidateFloor = FireFloorOf(local);
                if (collapsedFloors.Contains(candidateFloor)) continue;
                var distance = Mathf.Abs(local.x - x) + Mathf.Abs(candidateFloor - floor) * 3.5f;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = item;
            }
            return best;
        }

        private void SpawnFire(int floor, float x, float health)
        {
            var target = FindFireTarget(floor, x);
            if (target == null) return;
            var local = houseRoot.InverseTransformPoint(target.Body.position);
            floor = FireFloorOf(local);
            var visual = fireVfxPrefab != null ? Instantiate(fireVfxPrefab, houseRoot) : new GameObject("Yangın alevi");
            if (visual.transform.parent != houseRoot) visual.transform.SetParent(houseRoot, false);
            visual.name = "Yangın — " + target.displayName;
            visual.transform.localScale = Vector3.one * 0.42f;
            if (fireVfxPrefab == null)
            {
                var sprite = Icon("fire_flame");
                var renderer = visual.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = projectSpriteMaterial;
                if (sprite != null) visual.transform.localScale = new Vector3(0.65f / sprite.bounds.size.x, 0.9f / sprite.bounds.size.y, 1f);
            }
            SetParticleSimulationLocal(visual);
            GameObject smoke = null;
            if (smokeVfxPrefab != null)
            {
                smoke = Instantiate(smokeVfxPrefab, houseRoot);
                smoke.name = "Yanan eşya dumanı";
                smoke.transform.localScale = Vector3.one * 0.42f;
                ConvertSmokeMaterials(smoke);
                SetParticleSimulationLocal(smoke);
            }
            var fire = new FireSpot { visual = visual, smoke = smoke, target = target, health = health,
                floor = floor, x = local.x, spreadClock = 0f, burnClock = 0f };
            fires.Add(fire);
            UpdateFireVisual(fire);
        }

        private static void SetParticleSimulationLocal(GameObject root)
        {
            foreach (var particle in root.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particle.main;
                main.simulationSpace = ParticleSystemSimulationSpace.Local;
                main.loop = true;
                particle.Play(true);
            }
        }

        private void ConvertSmokeMaterials(GameObject smoke)
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) return;
            foreach (var renderer in smoke.GetComponentsInChildren<ParticleSystemRenderer>(true))
            {
                var original = renderer.sharedMaterial;
                if (original == null || original.shader == shader) continue;
                var material = new Material(shader);
                var texture = original.HasProperty("_MainTex") ? original.GetTexture("_MainTex") : null;
                if (texture != null) material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", new Color(0.45f, 0.43f, 0.43f, 0.48f));
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.SetOverrideTag("RenderType", "Transparent");
                material.renderQueue = 3000;
                renderer.material = material;
                fireRuntimeMaterials.Add(material);
            }
        }

        private void ReleaseFireMaterials()
        {
            foreach (var material in fireRuntimeMaterials) if (material != null) Destroy(material);
            fireRuntimeMaterials.Clear();
        }

        private void UpdateFireVisual(FireSpot fire)
        {
            if (fire.target == null || fire.target.Body == null) return;
            var local = houseRoot.InverseTransformPoint(fire.target.Body.position);
            fire.x = local.x;
            var box = fire.target.GetComponent<BoxCollider>();
            var top = box != null ? Mathf.Min(box.size.y * 0.32f, 0.5f) : 0.25f;
            if (fire.visual != null) fire.visual.transform.localPosition = new Vector3(local.x, local.y + top, -0.8f);
            if (fire.smoke != null) fire.smoke.transform.localPosition = new Vector3(local.x, local.y + top + 0.32f, -0.85f);
        }

        private void TickFireDisaster(float dt, Keyboard keyboard)
        {
            var spraying = equipment.Contains("yangin_tupu") && resident != null && !resident.IsClimbing &&
                ((keyboard != null && (keyboard.eKey.isPressed || keyboard.enterKey.isPressed)) || L1_TouchControls.ActionHeld);
            sprayingNow = spraying;
            PoseExtinguisher(spraying);
            ShowFoam(spraying);
            for (var i = fires.Count - 1; i >= 0; i--)
            {
                var fire = fires[i];
                if (fire.target == null || !fire.target.gameObject.activeInHierarchy)
                {
                    ClearFire(fire);
                    fires.RemoveAt(i);
                    continue;
                }
                UpdateFireVisual(fire);
                fire.spreadClock += dt;
                fire.burnClock += dt;
                if (spraying)
                {
                    var player = houseRoot.InverseTransformPoint(resident.transform.position);
                    var flame = houseRoot.InverseTransformPoint(fire.target.Body.position);
                    var forward = (flame.x - player.x) * resident.Facing;
                    if (forward >= -0.25f && forward <= 2.35f && Mathf.Abs(flame.y - player.y) < 1.35f)
                        fire.health -= dt * 85f;
                }
                if (fire.health <= 0f)
                {
                    ClearFire(fire);
                    fires.RemoveAt(i);
                    extinguishedFires++;
                    score += 300;
                    if (sfx != null) sfx.Play("level");
                    continue;
                }
                if (fire.spreadClock >= 5.5f && fires.Count < 8)
                {
                    fire.spreadClock = 0f;
                    var next = FindFireTarget(fire.floor, fire.x, 5.1f);
                    if (next != null)
                    {
                        var position = houseRoot.InverseTransformPoint(next.Body.position);
                        SpawnFire(FireFloorOf(position), position.x, 85f);
                        fireSpreads++;
                        Warn("Alevler " + next.displayName + " eşyasına sıçradı!");
                    }
                }
                if (fire.burnClock >= 12f)
                {
                    BurnFurniture(fire);
                    ClearFire(fire);
                    fires.RemoveAt(i);
                }
            }
            if (fires.Count == 0 && !floorCollapsePending) CompleteLevel();
        }

        private void BurnFurniture(FireSpot fire)
        {
            var item = fire.target;
            if (item == null) return;
            var id = item.GetInstanceID();
            burnedFurniture.Add(id);
            var local = houseRoot.InverseTransformPoint(item.Body.position);
            var ash = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ash.name = "Kül — " + item.displayName;
            ash.transform.SetParent(houseRoot, false);
            ash.transform.localPosition = new Vector3(local.x, FloorTops()[fire.floor] + 0.035f, -0.35f);
            ash.transform.localScale = new Vector3(0.38f, 0.025f, 0.3f);
            var collider = ash.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = ash.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null)
                {
                    var material = new Material(shader);
                    material.color = new Color(0.12f, 0.10f, 0.09f);
                    renderer.material = material;
                    Destroy(material, 10f);
                }
            }
            Destroy(ash, 9f);
            furniture.Remove(item);
            Destroy(item.gameObject);
            Warn(item.displayName + " yandı ve kül oldu!");
            CheckFloorBurnedOut(fire.floor);
        }

        private void CheckFloorBurnedOut(int floor)
        {
            if (collapsedFloors.Contains(floor) || !floorFireInventory.TryGetValue(floor, out var initial) || initial.Count == 0) return;
            foreach (var id in initial) if (!burnedFurniture.Contains(id)) return;
            collapsedFloors.Add(floor);
            var floorNumber = floor <= addedFloors ? addedFloors - floor : 0;
            var section = floorNumber > 0 ? houseRoot.Find("Alt kat " + floorNumber + " (yeni)") :
                houseRoot.Find(floor == addedFloors ? "Kat 1" : "Kat 2");
            if (section != null) section.gameObject.SetActive(false);
            if (resident != null && resident.ladders != null)
            {
                var top = FloorTops()[floor];
                foreach (var ladder in resident.ladders)
                {
                    if (ladder == null || ladder.bottom == null || ladder.top == null) continue;
                    var bottomY = houseRoot.InverseTransformPoint(ladder.bottom.position).y;
                    var topY = houseRoot.InverseTransformPoint(ladder.top.position).y;
                    if (Mathf.Abs(bottomY - top) < 0.7f || Mathf.Abs(topY - top) < 0.7f)
                        ladder.gameObject.SetActive(false);
                }
            }
            Warn((floor + 1) + ". kattaki bütün eşyalar yandı; kat çöktü!");
            if (!floorCollapsePending)
            {
                floorCollapsePending = true;
                StartCoroutine(FailAfterFloorCollapse());
            }
        }

        private IEnumerator FailAfterFloorCollapse()
        {
            yield return new WaitForSeconds(1.5f);
            if (state == RoundState.Disaster) Lose("Yangın bir katı tamamen yok etti.");
        }

        private void ClearFire(FireSpot fire)
        {
            if (fire.visual != null) Destroy(fire.visual);
            if (fire.smoke != null) Destroy(fire.smoke);
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
                foamVisual.transform.localScale = new Vector3(0.8f / sprite.bounds.size.x, 0.3f / sprite.bounds.size.y, 1f);
                var renderer = foamVisual.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;
                renderer.sharedMaterial = projectSpriteMaterial;
            }
            var player = houseRoot.InverseTransformPoint(resident.transform.position);
            foamVisual.transform.localPosition = new Vector3(player.x + resident.Facing * 0.72f, player.y + 0.10f, -1.5f);
            foamVisual.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 20f) * 7f);
            foamVisual.SetActive(true);
        }
    }
}
