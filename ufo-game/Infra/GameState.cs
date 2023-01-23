using System.Text.Json.Serialization;
using UfoGame.Model;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    [JsonInclude] public readonly TimelineData TimelineData;
    [JsonInclude] public readonly AccountingData AccountingData;
    [JsonInclude] public readonly StaffData StaffData;
    [JsonInclude] public readonly Archive Archive;
    [JsonInclude] public readonly MissionPrepData MissionPrepData;
    [JsonInclude] public readonly PendingMissions PendingMissions;
    [JsonInclude] public readonly Factions Factions;
    [JsonInclude] public readonly PlayerScoreData PlayerScoreData;
    [JsonInclude] public readonly ResearchData ResearchData;
    [JsonInclude] public readonly ProcurementData ProcurementData;
    [JsonInclude] public readonly ModalsState ModalsState;

    [JsonIgnore] public readonly PendingMission PendingMission;

    private readonly StateRefresh _stateRefresh;

    private readonly GameStateStorage _storage;

    public GameState(
        TimelineData timelineData,
        AccountingData accountingData,
        StaffData staffData,
        Archive archive,
        PendingMissions pendingMissions,
        PendingMission pendingMission,
        MissionPrepData missionPrepData,
        Factions factions,
        PlayerScoreData playerScoreData,
        ResearchData researchData,
        ProcurementData procurementData,
        GameStateStorage storage,
        StateRefresh stateRefresh,
        ModalsState modalsState)
    {
        TimelineData = timelineData;
        AccountingData = accountingData;
        StaffData = staffData;
        Archive = archive;
        PendingMissions = pendingMissions;
        PendingMission = pendingMission;
        MissionPrepData = missionPrepData;
        Factions = factions;
        PlayerScoreData = playerScoreData;
        ResearchData = researchData;
        ProcurementData = procurementData;
        _storage = storage;
        _stateRefresh = stateRefresh;
        ModalsState = modalsState;
    }

    public void PersistGameState()
        => _storage.Persist(this);

    public void Reset()
    {
        TimelineData.Reset();
        AccountingData.Reset();
        StaffData.Reset();
        Archive.Reset();
        MissionPrepData.Reset();
        Factions.Reset();
        PlayerScoreData.Reset();
        ResearchData.Reset();
        PendingMission.Reset();
        ProcurementData.Reset();
        ModalsState.Reset();
        _storage.Clear();
        _stateRefresh.Trigger();
    }
}