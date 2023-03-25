namespace UfoGameLib;

public record GameState(Timeline Timeline, Assets Assets, Archive Archive)
{
    protected GameState(GameState original)
    {
        Timeline = original.Timeline with { };
        Assets = original.Assets with { };
        Archive = original.Archive with { };
    }

    public bool GameOver => Assets.CurrentMoney < 0;
    public int NextAgentId => Assets.Agents.Count;

    public static GameState NewInitialGameState()
        => new GameState(
            new Timeline(CurrentTurn: 0),
            new Assets(CurrentMoney: 100, new Agents()),
            new Archive());
}