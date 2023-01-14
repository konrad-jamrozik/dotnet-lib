using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class GameState
{
    [JsonInclude] public readonly Timeline Timeline;
    [JsonInclude] public readonly Money Money;
    [JsonInclude] public readonly Staff Staff;
    [JsonInclude] public readonly OperationsArchive OperationsArchive;
    [JsonInclude] public readonly MissionPrep MissionPrep;
    [JsonInclude] public readonly PendingMission PendingMission;
    [JsonInclude] public readonly Factions Factions;
    [JsonInclude] public readonly PlayerScore PlayerScore;
    [JsonInclude] public readonly Research Research;
    [JsonInclude] public readonly ModalsState ModalsState;

    private readonly StateRefresh _stateRefresh;

    private readonly PersistentStorage _storage;

    public GameState(
        Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive operationsArchive,
        PendingMission pendingMission,
        MissionPrep missionPrep,
        Factions factions,
        PlayerScore playerScore,
        Research research,
        PersistentStorage storage,
        StateRefresh stateRefresh,
        ModalsState modalsState)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        OperationsArchive = operationsArchive;
        PendingMission = pendingMission;
        MissionPrep = missionPrep;
        Factions = factions;
        PlayerScore = playerScore;
        Research = research;
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
        Money.Data.Reset();
        Staff.Data.Reset();
        OperationsArchive.Reset();
        PendingMission.Data.Reset();
        MissionPrep.Data.Reset();
        Factions.Reset();
        PlayerScore.Data.Reset();
        Research.Reset();
        PendingMission.Reset();
        ModalsState.Reset();
        _storage.Reset();
        _stateRefresh.Trigger();
    }
}