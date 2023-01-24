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
    [JsonInclude] public readonly FactionsData FactionsData;
    [JsonInclude] public readonly AgentsData AgentsData;
    [JsonInclude] public readonly SickBayData SickBayData;
    [JsonInclude] public readonly PlayerScoreData PlayerScoreData;
    [JsonInclude] public readonly ResearchData ResearchData;
    [JsonInclude] public readonly ProcurementData ProcurementData;
    [JsonInclude] public readonly ModalsState ModalsState;

    private readonly PendingMission _pendingMission;
    private readonly Agents _agents;
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameStateStorage _storage;

    public GameState(
        TimelineData timelineData,
        AccountingData accountingData,
        StaffData staffData,
        Archive archive,
        PendingMissions pendingMissions,
        PendingMission pendingMission,
        MissionPrepData missionPrepData,
        FactionsData factionsData,
        AgentsData agentsData,
        Agents agents,
        SickBayData sickBayData,
        PlayerScoreData playerScoreData,
        ResearchData researchData,
        ProcurementData procurementData,
        GameStateStorage storage,
        ViewStateRefresh viewStateRefresh,
        ModalsState modalsState)
    {
        TimelineData = timelineData;
        AccountingData = accountingData;
        StaffData = staffData;
        Archive = archive;
        PendingMissions = pendingMissions;
        _pendingMission = pendingMission;
        MissionPrepData = missionPrepData;
        FactionsData = factionsData;
        AgentsData = agentsData;
        _agents = agents;
        SickBayData = sickBayData;
        PlayerScoreData = playerScoreData;
        ResearchData = researchData;
        ProcurementData = procurementData;
        _storage = storage;
        _viewStateRefresh = viewStateRefresh;
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
        FactionsData.Reset();
        SickBayData.Reset();
        PlayerScoreData.Reset();
        ResearchData.Reset();
        ProcurementData.Reset();
        ModalsState.Reset();
        _agents.Reset();
        _pendingMission.Reset();
        _storage.Clear();
        _viewStateRefresh.Trigger();
    }
}