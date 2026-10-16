# API Sözleşmesi

Kod karşılığı: `Assets/Scripts/Core/Contracts.cs` (namespace `Jam.Core`).
Bu sözleşme değişecekse önce PR veya takım mesajı ile mutabakat gerekir. Claude Code kendi başına public API değiştirmez.

```csharp
public enum GamePhase { Menu, Preparation, Disaster, Results, GameOver }
public enum DisasterType { Earthquake, Fire, Storm }

public interface IFurnitureItem {
    string ItemId { get; }
    float Mass { get; }
    int Value { get; }
    bool IsAnchored { get; }
    void SetAnchored(bool value);
}

public interface IBalanceService {
    float TiltDegrees { get; }
    float NormalizedDanger { get; } // 0 = güvenli, 1 = kritik
    void RegisterFurniture(IFurnitureItem item);
    void UnregisterFurniture(IFurnitureItem item);
    void RecalculateBalance();
}

public interface IDisasterService {
    bool IsRunning { get; }
    void StartEarthquake(float intensity, float duration);
    void StopDisaster();
}

public interface IPreparationService {
    int Budget { get; }
    bool TryPurchaseAnchoring(IFurnitureItem item);
    bool TryRemoveAnchoring(IFurnitureItem item);
}
```

## GameManager (Kişi 1, hazır)

| Üye | Açıklama |
|---|---|
| `GameManager.Instance.Phase` | Mevcut faz |
| `SetPhase(GamePhase)` | Faz değiştirir, `OnPhaseChanged` yayınlar |
| `static event Action<GamePhase> OnPhaseChanged` | Faz değişimi |
| `IsPaused`, `SetPaused(bool)`, `static event Action<bool> OnPauseChanged` | ESC ile duraklatma (yalnızca Preparation/Disaster'da) |
| `Restart()` | Sahneyi yeniden yükler, Preparation'a döner |

## Event sözleşmeleri

Sahibi olan servis tanımlar; isimler sabittir:

| Event | Sahibi |
|---|---|
| `OnPhaseChanged(GamePhase)` | GameManager (Kişi 1) |
| `OnFurnitureLost(string itemId)` | Physics (Kişi 2) |
| `OnDisasterStarted(DisasterType)` | Disasters (Kişi 2) |
| `OnDisasterEnded()` | Disasters (Kişi 2) |
| `OnBudgetChanged(int)` | Economy (Kişi 4) |
