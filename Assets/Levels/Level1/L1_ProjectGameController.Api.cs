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
    }
}
