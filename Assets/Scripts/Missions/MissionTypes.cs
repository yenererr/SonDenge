namespace Jam.Missions
{
    /// <summary>Görevin dünya içinde nasıl doğrulandığı (şartname §4.1).</summary>
    public enum TaskType
    {
        CarryToTarget,
        AnchorFurniture,
        InteractSwitch,
        InstallEquipment,
        ClearZone,
        RebalanceHouse,
        PlaceBarrier,
        ActivatePreInstalledDevice,
        DisasterAction
    }

    public enum TaskState { Pending, Completed, Missed }

    public enum WilsonMood { Normal, Heyecanli, Endiseli, Sevincli, Uzgun, Anlatici }

    /// <summary>Wilson'ın konuştuğu an (şartname §3.1).</summary>
    public enum WilsonTrigger
    {
        Intro,
        TaskPreview,
        PrepStart,
        TaskComplete,
        TimeWarning,
        DisasterWarning,
        ReportGood,
        ReportBad,
        Final
    }

    public enum LevelDisaster { Earthquake, Storm, Fire, Lightning, HeavyRain }
}
