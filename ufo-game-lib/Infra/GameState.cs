using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public record GameState(int Id, Timeline Timeline, Assets Assets, Missions Missions)
{
    // Notes on deep copying / cloning:
    // https://stackoverflow.com/a/222623/986533
    // https://stackoverflow.com/questions/63994473/how-to-copy-clone-records-in-c-sharp-9
    // See "Custom cloning" in https://www.c-sharpcorner.com/article/deep-dive-into-records-in-c-sharp-9/
    // kja instead of these ICloneable shenanigans, I could implement serialization to JSON and then
    // leverage it: step 1. serialize, step 2. deserialize into new object.
    protected GameState(GameState original)
    {
        Timeline = original.Timeline with { };
        Assets = original.Assets with { };
        Missions = (Missions)original.Missions.Clone();
        // Do I really need to clone everything? I guess agents and missions
        // that have been "archived" can be considered read-only and thus don't
        // need to be cloned any more. I will worry about this later, when
        // perf. benchmarking will prove that excessive cloning is on a hot path.
    }

    // kja for now, game ends in 10 turns, for testing purposes.
    public bool IsGameOver => Assets.CurrentMoney < 0 || Timeline.CurrentTurn > 10;
    public bool IsPast { get; set; } = false;
    public int NextAgentId => Assets.Agents.Count;
    public int NextMissionId => Missions.Count;
    

    public static GameState NewInitialGameState()
        => new GameState(
            Id: 0,
            new Timeline(CurrentTurn: 0),
            new Assets(CurrentMoney: 100, new Agents(), TransportCapacity: 4),
            new Missions());
}