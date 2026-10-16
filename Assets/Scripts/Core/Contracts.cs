namespace Jam.Core
{
    // Entegrasyon sözleşmesi (docs/API_CONTRACTS.md).
    // Değiştirmeden önce takımla mutabakat gerekir; AI ajanları bu dosyayı kendi başına değiştirmez.

    public enum GamePhase { Menu, Preparation, Disaster, Results, GameOver }
    public enum DisasterType { Earthquake, Fire, Storm }

    public interface IFurnitureItem
    {
        string ItemId { get; }
        float Mass { get; }
        int Value { get; }
        bool IsAnchored { get; }
        void SetAnchored(bool value);
    }

    public interface IBalanceService
    {
        float TiltDegrees { get; }
        float NormalizedDanger { get; } // 0 = güvenli, 1 = kritik
        void RegisterFurniture(IFurnitureItem item);
        void UnregisterFurniture(IFurnitureItem item);
        void RecalculateBalance();
    }

    public interface IDisasterService
    {
        bool IsRunning { get; }
        void StartEarthquake(float intensity, float duration);
        void StopDisaster();
    }

    public interface IPreparationService
    {
        int Budget { get; }
        bool TryPurchaseAnchoring(IFurnitureItem item);
        bool TryRemoveAnchoring(IFurnitureItem item);
    }
}
