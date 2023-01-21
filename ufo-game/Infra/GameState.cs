using System.Text.Json.Serialization;
using UfoGame.Model;
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
    [JsonInclude] public readonly PendingMission PendingMission;
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
        PendingMission pendingMission,
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
        PendingMission = pendingMission;
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
        // kja classes for whom the .Data. is being reset probably don't need
        // to be included in the GameState. Only classes in Model/Data dir need to be.
        Timeline.Reset();
        MoneyData.Reset();
        StaffData.Reset();
        Archive.Reset();
        MissionPrepData.Reset();
        Factions.Reset();
        PlayerScoreData.Reset();
        Research.Reset();
        PendingMission.Reset();
        ProcurementData.Reset();
        ModalsState.Reset();
        _storage.Reset();
        _stateRefresh.Trigger();
    }
}