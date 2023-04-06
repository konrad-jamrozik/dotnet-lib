namespace UfoGameLib.Infra;

public class PlayerActions : List<PlayerAction>
{
    public PlayerActions(IEnumerable<PlayerAction> actions) : base(actions)
    {
    }

    public void Apply(GameState state)
    {
        ForEach(action => action.Apply(state));
    }
}