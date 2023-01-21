using System.Text.Json.Serialization;
using UfoGame.Model;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class GameState
{
    [JsonInclude] public readonly Timeline Timeline;
    [JsonInclude] public readonly Money Money;
    [JsonInclude] public readonly Staff Staff;
    [JsonInclude] public readonly Archive Archive;
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
        Archive archive,
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
        Archive = archive;
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
        // kja classes for whom the .Data. is being reset probably don't need
        // to be included in the GameState. Only classes in Model/Data dir need to be.
        Timeline.Reset();
        Money.Data.Reset();
        Staff.Data.Reset();
        Archive.Reset();
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