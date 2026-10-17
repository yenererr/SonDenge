using System.Collections.Generic;
using System.Text;
using Jam.UI;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Level 1 depremi: değişken yatay ivme, kayıp eşya takibi ve AFET RAPORU.
    /// Sayıların tamamı Inspector'dan ayarlanır. Gerçek deprem tavsiyesi değil; oyun simülasyonudur.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        [Header("Level 1 depremi")]
        [Tooltip("Deprem süresi (sn). 10–25 arası önerilir.")]
        [Range(5f, 25f)] public float quakeDuration = 10f;
        [Tooltip("En yüksek yatay ivme (m/sn²); serbest eşyalara uygulanır.")]
        public float quakeAcceleration = 4.5f;
        public float quakeFrequencyA = 1.3f;
        public float quakeFrequencyB = 2.9f;
        [Range(0f, 1f)] public float quakeNoise = 0.35f;
        [Tooltip("Zaman içinde şiddet: 0 = başlangıç, 1 = bitiş.")]
        public AnimationCurve quakeEnvelope = new AnimationCurve(
            new Keyframe(0f, 0.35f), new Keyframe(0.18f, 1f), new Keyframe(0.7f, 0.9f), new Keyframe(1f, 0f));
        [Tooltip("İvmenin evin eğimine eklediği pay (radyan / m/sn²).")]
        public float quakeTiltPerAcceleration = 0.012f;
        public L1_CameraController cameraRig;
        public float cameraShakeStrength = 0.12f;
        [Header("Rapor")]
        [Tooltip("Sonraki bölüm sahnesi. Bölüm hazır değilse 'Yakında' gösterilir.")]
        public string nextLevelScene = "Level2";
        public bool nextLevelReady = false;
        [Tooltip("Bu kütle ve üstü 'ağır eşya' sayılır (kg).")]
        public float heavyMassKg = 50f;
        public int levelCompleteBonus = 450;

        private sealed class QuakeRecord
        {
            public L1_MovableProp item;
            public string name;
            public float mass;
            public int value;
            public bool anchored;
            public float startX;
            public bool lost;
        }

        private readonly List<QuakeRecord> quakeRecords = new List<QuakeRecord>();
        private float quakeStartTime, quakePhase, shakeClock;
        private bool levelRewardGiven, houseSurvived;
        private int safetyScore;

        private void BeginQuakeRecord()
        {
            quakeRecords.Clear();
            maxTilt = 0f;
            quakeStartTime = Time.time;
            quakePhase = Random.Range(0f, Mathf.PI * 2f);
            foreach (var item in furniture)
            {
                if (item == null || !item.gameObject.activeInHierarchy) continue;
                quakeRecords.Add(new QuakeRecord
                {
                    item = item,
                    name = PlayerFacingName(item),
                    mass = item.Mass,
                    value = CatalogCost(item),
                    anchored = item.IsAnchored,
                    startX = houseRoot.InverseTransformPoint(item.transform.position).x
                });
            }
        }

        private static ShopItem CatalogEntry(L1_MovableProp item)
        {
            if (item == null) return null;
            foreach (var entry in Catalog)
                if ((item.itemId != null && item.itemId.StartsWith(entry.id)) ||
                    (entry.template != null && item.displayName != null && item.displayName.Contains(entry.template)))
                    return entry;
            return null;
        }

        private static int CatalogCost(L1_MovableProp item)
        {
            var entry = CatalogEntry(item);
            return entry != null ? entry.cost : Mathf.Max(10, item.Value);
        }

        /// <summary>Oyuncuya gösterilen Türkçe ad (katalog başlığı; yoksa sahnedeki ad).</summary>
        public static string PlayerFacingName(L1_MovableProp item)
        {
            var entry = CatalogEntry(item);
            return entry != null ? entry.title : item != null ? item.displayName : "";
        }

        /// <summary>İki sinüs + sınırlı gürültü, zarf eğrisiyle güçlenip söner (m/sn²).</summary>
        private float QuakeAcceleration()
        {
            var t = Time.time - quakeStartTime;
            var progress = Mathf.Clamp01(t / Mathf.Max(0.1f, quakeDuration));
            var wave = 0.65f * Mathf.Sin(2f * Mathf.PI * quakeFrequencyA * t + quakePhase) +
                0.35f * Mathf.Sin(2f * Mathf.PI * quakeFrequencyB * t + quakePhase * 1.7f);
            var noise = (Mathf.PerlinNoise(t * 3.1f, quakePhase) - 0.5f) * 2f * quakeNoise;
            return Mathf.Clamp(wave + noise, -1.2f, 1.2f) * quakeAcceleration * quakeEnvelope.Evaluate(progress);
        }

        private void TrackLostFurniture()
        {
            foreach (var record in quakeRecords)
            {
                if (record.lost || record.item == null) continue;
                var local = houseRoot.InverseTransformPoint(record.item.transform.position);
                // Zeminden düşen ya da evin dışına kayan eşya bir kez kayıp sayılır.
                if (local.y < lowestFloorTop - 1.45f || Mathf.Abs(local.x) > roomHalfWidth + 1.47f) record.lost = true;
            }
        }

        private void UpdateQuakeCamera()
        {
            if (state != RoundState.Disaster || level != 1) return;
            if (cameraRig == null && sceneCamera != null) cameraRig = sceneCamera.GetComponent<L1_CameraController>();
            if (cameraRig == null) return;
            shakeClock -= Time.deltaTime;
            if (shakeClock > 0f) return;
            shakeClock = 0.2f;
            var strength = cameraShakeStrength * Mathf.Abs(QuakeAcceleration()) / Mathf.Max(0.1f, quakeAcceleration);
            if (GameSettings.ReduceShake) strength *= 0.2f;
            cameraRig.Shake(0.25f, strength);
        }

        private void CompleteEarthquakeLevel(bool survived, string reason)
        {
            if (state != RoundState.Disaster) return;
            houseSurvived = survived;
            TrackLostFurniture();
            var totalValue = 0;
            var protectedValue = 0;
            var lostHeavy = 0;
            var anchoredHeavy = 0;
            var lostNames = new List<string>();
            var anchoredNames = new List<string>();
            foreach (var record in quakeRecords)
            {
                totalValue += record.value;
                if (!record.lost) protectedValue += record.value;
                if (record.anchored) anchoredNames.Add(record.name);
                if (record.mass >= heavyMassKg && record.anchored) anchoredHeavy++;
                if (record.lost)
                {
                    lostNames.Add(record.name);
                    if (record.mass >= heavyMassKg) lostHeavy++;
                }
            }
            // Puan taslağı (plan §10.4); katsayılar ileride Inspector'a taşınabilir.
            var baseSafety = survived ? 40 : 0;
            var anchorBonus = Mathf.Min(30, 10 * anchoredHeavy);
            var protectedBonus = Mathf.RoundToInt(20f * protectedValue / Mathf.Max(1, totalValue));
            var balanceBonus = Mathf.Max(0, 10 - Mathf.RoundToInt(maxTilt / 3f));
            var penalty = 10 * lostHeavy;
            safetyScore = Mathf.Clamp(baseSafety + anchorBonus + protectedBonus + balanceBonus - penalty, 0, 100);

            var text = new StringBuilder();
            text.AppendLine(survived ? "Ev ayakta kaldı." : "Ev dengesini kaybetti: " + reason);
            text.AppendLine("Sabitlenen eşya: " + (anchoredNames.Count == 0 ? "yok" : string.Join(", ", anchoredNames)));
            text.AppendLine("Korunan eşya: " + (quakeRecords.Count - lostNames.Count) + " / " + quakeRecords.Count +
                (lostNames.Count > 0 ? "   ·   Kaybolan: " + string.Join(", ", lostNames) : ""));
            text.AppendLine("En yüksek eğim: " + maxTilt.ToString("F1") + "°   ·   Hazırlık puanı: " + safetyScore + " / 100");
            if (survived && !levelRewardGiven)
            {
                levelRewardGiven = true;
                score += safetyScore;
                text.AppendLine("Kazanılan para: +" + levelCompleteBonus + " altın (sonraki bölüme aktarılır)");
            }
            text.AppendLine();
            text.Append(anchoredHeavy > 0
                ? "Ağır eşyayı önceden sabitlemek işe yaradı. Gerçek hayatta da dolapları duvara sabitleyin."
                : lostHeavy > 0
                    ? "Sabitlenmemiş ağır eşya devrildi. Gerçek hayatta ağır eşyaları önceden duvara sabitleyin."
                    : "Gerçek hayatta: ağır eşyaları alta koyun ve duvara sabitleyin.");
            text.Append("\nDeprem anında eşya taşımayın: ÇÖK – KAPAN – TUTUN.");
            report = text.ToString();
            marketOpen = false;
            SetMusic(planningMusic);
            SetState(survived ? RoundState.Report : RoundState.GameOver);
            if (sfx != null) sfx.Play(survived ? "level" : "defeat");
        }

        private void GoToNextLevel()
        {
            if (!nextLevelReady) { Warn("Sonraki bölüm yakında."); return; }
            Time.timeScale = 1f;
            if (Jam.Core.SceneLoader.Instance != null) Jam.Core.SceneLoader.Instance.Load(nextLevelScene);
        }

        private void DrawEarthquakeReport(float width, float height, bool survived)
        {
            Shade(width, height);
            var rect = new Rect((width - 640f) * 0.5f, (height - 470f) * 0.5f, 640f, 470f);
            Panel(rect, Stone, survived ? Green : Red);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 16f, 590f, 40f), "AFET RAPORU", uiTitle);
            GUI.Label(new Rect(rect.x + 25f, rect.y + 56f, 590f, 28f),
                survived ? "EV AYAKTA KALDI" : "DENGE BOZULDU", uiCentered);
            L1_PaperGui.Rule(rect.x + 90f, rect.y + 88f, 460f, survived ? Green : Red);
            Panel(new Rect(rect.x + 24f, rect.y + 98f, 592f, 280f), Deep, Border);
            GUI.Label(new Rect(rect.x + 40f, rect.y + 108f, 560f, 262f), report, uiText);
            var y = rect.y + 396f;
            if (ActionButton(new Rect(rect.x + 24f, y, 186f, 48f), "TEKRAR DENE", Border)) Restart();
            if (ActionButton(new Rect(rect.x + 227f, y, 186f, 48f),
                    nextLevelReady && survived ? "SONRAKİ BÖLÜM" : "SONRAKİ (YAKINDA)", survived && nextLevelReady ? Green : Muted)
                && survived) GoToNextLevel();
            if (ActionButton(new Rect(rect.x + 430f, y, 186f, 48f), "ANA MENÜ", Red)) ReturnToMainMenu();
        }
    }
}
