namespace UfoGameLib;

public record GameState(Timeline Timeline, Archive Archive)
{
    protected GameState(GameState original)
    {
        Timeline = original.Timeline with { };
        Archive = original.Archive with { };
    }
}