namespace UfoGameLib.Infra;

public abstract class PlayerAction
{
    public abstract void Apply(GameState state);
}