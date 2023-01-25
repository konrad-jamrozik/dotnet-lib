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
    [JsonInclude] public readonly ArchiveData ArchiveData;
    [JsonInclude] public readonly MissionPrepData MissionPrepData;
    [JsonInclude] public readonly PendingMissionsData PendingMissionsData;
    [JsonInclude] public readonly FactionsData FactionsData;
    [JsonInclude] public readonly AgentsData AgentsData;
    [JsonInclude] public readonly SickBayData SickBayData;
    [JsonInclude] public readonly PlayerScoreData PlayerScoreData;
    [JsonInclude] public readonly ResearchData ResearchData;
    [JsonInclude] public readonly ProcurementData ProcurementData;
    [JsonInclude] public readonly ModalsState ModalsState;
    public readonly List<IDeserializable> Deserializables;
    public readonly List<IResettable> Resettables;

    private readonly PendingMission _pendingMission;
    private readonly Agents _agents;
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameStateStorage _storage;

    public GameState(
        TimelineData timelineData,
        AccountingData accountingData,
        StaffData staffData,
        ArchiveData archiveData,
        PendingMissionsData pendingMissionsData,
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
        ModalsState modalsState,
        IEnumerable<IDeserializable> deserializables,
        IEnumerable<IResettable> resettables)
    {
        TimelineData = timelineData;
        AccountingData = accountingData;
        StaffData = staffData;
        ArchiveData = archiveData;
        PendingMissionsData = pendingMissionsData;
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
        Deserializables = deserializables.ToList();
        Resettables = resettables.ToList();
    }

    // kja instead use reflection to persist all classes that implement IDeserializable.
    // Then rename IDeserializable to IPersistable.
    // This will require figuring out how to ensure the json has correct format.
    public void PersistGameState()
        => _storage.Persist(this);

    public void Reset()
    {
        Resettables.ForEach(resettable => resettable.Reset());

        _storage.Clear();

        _viewStateRefresh.Trigger();
    }
}