using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// Bölüm 5 (görev modu): yağmur suyu alt kattan başlayıp katları yavaş yavaş basar.
    /// Kum torbası bariyeri yükselmeyi çok yavaşlatır; su orta kata ulaşırsa ev su basmış sayılır.
    /// Bu fiziksel bir sel simülasyonu değil, oyunlaştırılmış bir gösterimdir.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        [Header("Sel (Bölüm 5)")]
        [Tooltip("Bariyer yokken su yükselme hızı (m/sn).")]
        public float floodRiseNoBarrier = 0.12f;
        [Tooltip("Kum torbası bariyeri varken su yükselme hızı (m/sn).")]
        public float floodRiseWithBarrier = 0.032f;
        [Tooltip("Su, alt kat zemininin bu kadar altından başlar (m).")]
        public float floodStartBelowFloor = 0.05f;
        [Tooltip("Suyun evin önünde (kameraya doğru) başladığı z ve derinliği.")]
        public float floodFrontZ = -1.75f;
        public float floodDepth = 3.6f;

        private L1_FloodWater floodWater;
        private float floodHeight;

        /// <summary>Su yüksekliği, alt kat zemininden (m). Negatif: henüz zemine ulaşmadı.</summary>
        public float FloodHeight => floodHeight;

        private void BeginFlood()
        {
            floodHeight = -floodStartBelowFloor;
            if (floodWater == null)
            {
                var halfWidth = (drag != null ? drag.roomHalfWidth : 4.3f) - 0.05f;
                floodWater = L1_FloodWater.Create(houseRoot, FloorTops()[0] - 0.02f, halfWidth, floodFrontZ, floodDepth);
            }
            floodWater.SetVisible(true);
            floodWater.SetTop(FloorTops()[0] + floodHeight);
        }

        private void UpdateFlood(float dt)
        {
            if (floodWater == null) BeginFlood();
            var rate = equipment.Contains("sandbags") ? floodRiseWithBarrier : floodRiseNoBarrier;
            floodHeight += rate * dt;
            floodWater.SetTop(FloorTops()[0] + floodHeight);
            // Rapor/puan için: alt katın ne kadarı su altında (100 = su orta kat zeminine ulaştı).
            indoorFlood = Mathf.Clamp(floodHeight / floorHeight * 100f, 0f, 100f);
            water = Mathf.Max(0f, floodHeight * 100f);
            if (indoorFlood >= 100f) Lose("Su orta kata ulaştı; ev su bastı.");
        }

        private void HideFlood()
        {
            floodHeight = -floodStartBelowFloor;
            if (floodWater != null) floodWater.SetVisible(false);
        }
    }
}
