namespace UfoGameLib;

public class PlayerActions
{
    public List<PlayerAction> Actions;

    public PlayerActions(params PlayerAction[] actions)
    {
        Actions = actions.ToList();
    }

    public GameState Apply(GameState gameState)
    {
        GameState gameStateCopy = gameState with { };
        Actions.ForEach(action => action.Apply(gameStateCopy));
        return gameStateCopy;
    }
}