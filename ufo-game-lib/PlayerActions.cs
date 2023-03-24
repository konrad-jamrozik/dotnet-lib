namespace UfoGameLib;

public class PlayerActions
{
    public List<PlayerAction> Actions;

    public PlayerActions(params PlayerAction[] actions)
    {
        Actions = actions.ToList();
    }
}