using System.Text.Json.Serialization;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    [JsonInclude] public readonly Timeline Timeline;
    [JsonInclude] public readonly MoneyData MoneyData;
    [JsonInclude] public readonly StaffData StaffData;
    [JsonInclude] public readonly Archive Archive;
    [JsonInclude] public readonly MissionPrepData MissionPrepData;
    [JsonInclude] public readonly PendingMissions PendingMissions;
    [JsonInclude] public readonly Factions Factions;
    [JsonInclude] public readonly PlayerScoreData PlayerScoreData;
    [JsonInclude] public readonly Research Research;
    [JsonInclude] public readonly ProcurementData ProcurementData;
    [JsonInclude] public readonly ModalsState ModalsState;

    private readonly StateRefresh _stateRefresh;

    private readonly PersistentStorage _storage;

    public GameState(
        Timeline timeline,
        MoneyData moneyData,
        StaffData staffData,
        Archive archive,
        PendingMissions pendingMissions,
        MissionPrepData missionPrepData,
        Factions factions,
        PlayerScoreData playerScoreData,
        Research research,
        ProcurementData procurementData,
        PersistentStorage storage,
        StateRefresh stateRefresh,
        ModalsState modalsState)
    {
        Timeline = timeline;
        MoneyData = moneyData;
        StaffData = staffData;
        Archive = archive;
        PendingMissions = pendingMissions;
        MissionPrepData = missionPrepData;
        Factions = factions;
        PlayerScoreData = playerScoreData;
        Research = research;
        ProcurementData = procurementData;
        _storage = storage;
        _stateRefresh = stateRefresh;
        ModalsState = modalsState;
    }

    public void PersistGameState()
    {
        Console.Out.WriteLine("Persisting game state");
        _storage.SetItem("GameState", this);
    }

    public void Reset()
    {
        Timeline.Reset();
        MoneyData.Reset();
        StaffData.Reset();
        Archive.Reset();
        MissionPrepData.Reset();
        Factions.Reset();
        PlayerScoreData.Reset();
        Research.Reset();
        PendingMissions.Reset();
        ProcurementData.Reset();
        ModalsState.Reset();
        _storage.Reset();
        _stateRefresh.Trigger();
    }
}