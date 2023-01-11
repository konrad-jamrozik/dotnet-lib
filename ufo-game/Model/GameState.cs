using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class GameState
{
    [JsonInclude]
    public readonly Timeline Timeline;
    [JsonInclude]
    public readonly Money Money;
    [JsonInclude]
    public readonly Staff Staff;
    [JsonInclude]
    public readonly OperationsArchive OperationsArchive;
    [JsonInclude]
    public readonly MissionPrepData MissionPrepData;
    [JsonInclude]
    public readonly PendingMission PendingMission;
    [JsonInclude]
    public readonly Factions Factions;
    [JsonInclude]
    public readonly PlayerScore PlayerScore;
    [JsonInclude]
    public readonly Research Research;

    private readonly StateRefresh _stateRefresh;

    private readonly PersistentStorage _storage;

    public GameState(
        Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive operationsArchive,
        PendingMission pendingMission,
        MissionPrepData missionPrepData,
        Factions factions,
        PlayerScore playerScore,
        Research research,
        PersistentStorage storage,
        StateRefresh stateRefresh)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        OperationsArchive = operationsArchive;
        PendingMission = pendingMission;
        MissionPrepData = missionPrepData;
        Factions = factions;
        PlayerScore = playerScore;
        Research = research;
        _storage = storage;
        _stateRefresh = stateRefresh;
    }

    public void PersistGameState()
    {
        Console.Out.WriteLine("Persisting game state");
        _storage.SetItem("Game", this);
    }

    public void Reset()
    {
        _storage.Reset();
        // kja NEXT need to add resets for everything here
        Timeline.Reset();
        _stateRefresh.Trigger();
    }
}