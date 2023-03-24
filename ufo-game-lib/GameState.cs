namespace UfoGameLib;

public record GameState(Archive Archive)
{
    protected GameState(GameState original) => Archive = original.Archive with {};
}