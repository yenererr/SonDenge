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
        public bool HasModal => (marketOpen && useLegacyHud) || menuOpen || howToOpen || briefingOpen || state == RoundState.Report ||
            state == RoundState.GameOver || state == RoundState.Victory || state == RoundState.StartMenu;
        public int Budget => budget;
        public float TimeLeft => Mathf.Max(0f, timer);
        public float PlanningDuration => PreparationSeconds;
        /// <summary>Pozitif değer: evin sağ tarafı aşağıda.</summary>
        public float TiltDegrees => -houseAngle * Mathf.Rad2Deg;
        public float CollapseAngle => collapseAngle;
        public string WarningText => warningUntil > 0f ? warning : null;
        public int AnchorCost => 30;
        public L1_MovableProp SelectedItem => drag != null ? drag.Selected : null;
        public Sprite UiLevelIcon => Icon(level == 1 ? "deprem_cantasi" : level == 2 ? "storm_brace" :
            level == 3 ? "fire_flame" : level == 4 ? "lightning_rod_system" : "water_sensor");
        public Sprite UiSelectedIcon
        {
            get
            {
                var selected = SelectedItem;
                if (selected == null) return null;
                foreach (var entry in Catalog)
                    if ((selected.itemId != null && selected.itemId.StartsWith(entry.id)) ||
                        (entry.template != null && selected.displayName != null && selected.displayName.Contains(entry.template)))
                        return Icon(entry.id);
                return null;
            }
        }
        public string UiSelectedDescription
        {
            get
            {
                var selected = SelectedItem;
                if (selected == null) return string.Empty;
                foreach (var entry in Catalog)
                    if ((selected.itemId != null && selected.itemId.StartsWith(entry.id)) ||
                        (entry.template != null && selected.displayName != null && selected.displayName.Contains(entry.template)))
                        return Description(entry);
                return "Evdeki taşınabilir eşya.";
            }
        }

        public void UiOpenMarket() { if (state == RoundState.Planning) marketOpen = true; }
        public void UiToggleMarket() { if (state == RoundState.Planning) marketOpen = !marketOpen; }
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
            public int cost, mass, unlockLevel, income;
            public Sprite icon;
            public bool locked, installed, affordable;
        }

        public string[] UiCategories => System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Take(Categories, Mathf.Min(Categories.Length, level + 1)));
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
                    income = item.category == "Mobilya" ? FurnitureIncome(item) : 0,
                    icon = Icon(item.id),
                    locked = IsLocked(item),
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
        /// <summary>Yeni Duraklat menüsünde ayarlar/onay açıkken ESC duraklatmayı kapatmaz (alt panel kapanır).</summary>
        public bool PauseSubMenuOpen { get; set; }
        /// <summary>true: duraklatma menüsünü PauseMenuView prefab'ı gösterir, eski kodla kurulan panel gizlenir.</summary>
        public bool ExternalPauseMenu { get; set; }
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
        public bool NextLevelAvailable => state == RoundState.Report &&
            (missionMode ? HasNextScene : growDownward ? level < 5 : nextLevelReady);
        public void UiRestart() => Restart();
        public void UiMainMenu() => ReturnToMainMenu();
        public void UiNextLevel()
        {
            if (state != RoundState.Report) return;
            if (missionMode) GoToNextMissionScene();
            else if (growDownward) Advance(); else GoToNextLevel();
        }
        public int LevelNumber => level;
        /// <summary>Görev modunda radyo yalnız gerçek hayatla çelişmeyen hazırlık uyarısı verir.</summary>
        private string MissionRadioLine => level == 1 ? "RADYO SİVRİTEPE — Bu gece sarsıntı bekleniyor. Hazırlan!" :
            level == 2 ? "RADYO SİVRİTEPE — Sert fırtına geliyor. Pencereleri kapatın, camlardan uzak durun!" :
            level == 3 ? "RADYO SİVRİTEPE — Yangın riski yüksek. Duman alarmını ve kaçış yolunu kontrol edin!" :
            level == 4 ? "RADYO SİVRİTEPE — Yıldırım uyarısı. Cihazların fişini fırtınadan önce çekin!" :
            "RADYO SİVRİTEPE — Şiddetli yağmur geliyor. Girişi ve drenajı önceden hazırlayın!";
        public int TasksDone => missions != null ? missions.CompletedCount : 0;
        public int TasksTotal => missions != null ? missions.TotalCount : 0;
        public string DisasterName => missionMode && levelDefinition != null ? levelDefinition.disasterNameTr : "DEPREM";
        /// <summary>Afet sırasında HUD'da gösterilen gerçek güvenlik satırı.</summary>
        public string DisasterSafetyLine
        {
            get
            {
                var line = levelDefinition != null && levelDefinition.safetyWarningsTr.Length > 0
                    ? string.Join("  ·  ", levelDefinition.safetyWarningsTr) : "";
                if (level == 3) line += equipment.Contains("exit_clear")
                    ? "  ·  Yangın büyürse T: açık yoldan TAHLİYE"
                    : "  ·  Kaçış yolu kapalı!";
                if (level == 3 && equipment.Contains("yangin_tupu")) line += "  ·  Küçük alevde E: söndür";
                return line;
            }
        }
        /// <summary>Hazırlık süresi bitti; HUD 3-2-1 geri sayımını onay sormadan başlatır.</summary>
        public event System.Action ForcedDisasterRequested;
        public string LevelTitle => missionMode && levelDefinition != null ? levelDefinition.titleTr : level == 1 ? "İLK SARSINTI" : level == 2 ? "DAĞ FIRTINASI" : level == 3 ? "YANGIN" : level == 4 ? "YILDIRIM" : "ŞİDDETLİ YAĞMUR";
        public string RadioLine => missionMode ? MissionRadioLine : level == 1 ? "RADYO SİVRİTEPE — Bu gece sarsıntı bekleniyor. Hazırlan!" :
            level == 2 ? "RADYO SİVRİTEPE — Dağdan sert fırtına geliyor. Ağır eşyaları rüzgârın tersine al!" :
            level == 3 ? "RADYO SİVRİTEPE — Yangın riski yüksek. Yangın tüpünü hazır et!" :
            level == 4 ? "RADYO SİVRİTEPE — Zirvede yıldırım uyarısı. Paratoner kur!" :
            "RADYO SİVRİTEPE — Şiddetli yağmur bekleniyor. Suyu evden uzaklaştır!";
    }
}
