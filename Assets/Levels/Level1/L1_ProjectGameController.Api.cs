using System.Collections.Generic;
using UnityEngine;

namespace Jam.Levels.Level1
{
    /// <summary>
    /// uGUI HUD'un (L1_Hud) okuduğu durum ve çağırdığı eylemler. Oyun kuralları değişmez;
    /// HUD yalnız bu yüzeyi kullanır, böylece eski IMGUI ile ikinci bir durum makinesi oluşmaz.
    /// </summary>
    public sealed partial class L1_ProjectGameController
    {
        public bool IsPlanning => state == RoundState.Planning;
        public bool IsDisaster => state == RoundState.Disaster;
        public bool IsPausedMenu => menuOpen || howToOpen;
        public bool IsMarketOpen => marketOpen;
        public bool HasModal => marketOpen || menuOpen || howToOpen || state == RoundState.Report ||
            state == RoundState.GameOver || state == RoundState.Victory || state == RoundState.StartMenu;
        public int Budget => budget;
        public float TimeLeft => Mathf.Max(0f, timer);
        public float PlanningDuration => 45f;
        /// <summary>Pozitif değer: evin sağ tarafı aşağıda.</summary>
        public float TiltDegrees => -houseAngle * Mathf.Rad2Deg;
        public float CollapseAngle => collapseAngle;
        public string WarningText => warningUntil > 0f ? warning : null;
        public int AnchorCost => 30;
        public L1_MovableProp SelectedItem => drag != null ? drag.Selected : null;

        public void UiOpenMarket() { if (state == RoundState.Planning) marketOpen = true; }
        public void UiStartDisaster() => StartDisaster();
        /// <summary>Geri sayımdan önce kontrol; başlatılamıyorsa uyarıyı gösterir ve false döner.</summary>
        public bool UiCanStartDisaster() => CanStartDisaster(false);
        /// <summary>Geri sayım sırasında sürükleme ve hazırlık saati durur.</summary>
        public bool CountdownActive
        {
            get => countdownActive;
            set { countdownActive = value; if (drag != null && value) drag.DropCarried(true); }
        }
        private bool countdownActive;
        /// <summary>Boşluk tuşu bu olayı tetikler; HUD geri sayımı başlatır.</summary>
        public event System.Action DisasterRequested;
        public void UiToggleAnchor() { if (state == RoundState.Planning) ToggleAnchor(); }
        public void UiSellSelected() { if (state == RoundState.Planning) SellSelected(); }
        public void UiTogglePause() => ToggleMenu();

        // ---------------------------------------------------------------- mağaza
        public struct ShopEntry
        {
            public string id, title, description;
            public int cost, mass, unlockLevel;
            public bool locked, installed, affordable;
        }

        public string[] UiCategories => singleFloorLevel ? new[] { Categories[0], Categories[1] } : Categories;
        public int AnchorStock => anchors;

        public List<ShopEntry> UiShopEntries(int categoryIndex)
        {
            var result = new List<ShopEntry>();
            var categories = UiCategories;
            if (categoryIndex < 0 || categoryIndex >= categories.Length) return result;
            foreach (var item in Catalog)
            {
                if (item.category != categories[categoryIndex]) continue;
                result.Add(new ShopEntry
                {
                    id = item.id, title = item.title, description = Description(item),
                    cost = item.cost, mass = item.mass, unlockLevel = item.unlockLevel,
                    locked = level < item.unlockLevel,
                    installed = item.category != "Mobilya" && item.id != "sabitleme_aparati" && equipment.Contains(item.id),
                    affordable = budget >= item.cost
                });
            }
            return result;
        }

        public void UiPurchase(string id)
        {
            foreach (var item in Catalog)
                if (item.id == id) { Purchase(item); return; }
        }

        public void UiCloseMarket() => marketOpen = false;

        // ---------------------------------------------------------------- duraklatma
        public bool IsMenuOpen => menuOpen;
        public bool IsHowToOpen => howToOpen;
        public bool MusicMuted => musicMuted;
        public void UiToggleMusic() { musicMuted = !musicMuted; SyncPause(); }
        public void UiOpenHowTo() { howToOpen = true; SyncPause(); }
        public void UiCloseHowTo() { howToOpen = false; SyncPause(); }

        // ---------------------------------------------------------------- rapor
        public bool IsReport => state == RoundState.Report;
        public bool IsGameOver => state == RoundState.GameOver;
        public string ReportText => report;
        public int SafetyScore => safetyScore;
        public bool NextLevelAvailable => nextLevelReady && state == RoundState.Report;
        public void UiRestart() => Restart();
        public void UiMainMenu() => ReturnToMainMenu();
        public void UiNextLevel() { if (state == RoundState.Report) GoToNextLevel(); }
    }
}
