namespace UfoGameLib;

public record GameState(Timeline Timeline, Assets Assets, Missions Missions)
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
    }

    public bool GameOver => Assets.CurrentMoney < 0;
    public int NextAgentId => Assets.Agents.Count;
    public int NextMissionId => Missions.Count;

    public static GameState NewInitialGameState()
        => new GameState(
            new Timeline(CurrentTurn: 0),
            new Assets(CurrentMoney: 100, new Agents()),
            new Missions());
}