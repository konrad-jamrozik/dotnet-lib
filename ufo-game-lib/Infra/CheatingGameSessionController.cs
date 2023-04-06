namespace UfoGameLib.Infra;

/// <summary>
/// A GameSessionController that allows for cheating by making it possible to:
/// - view entire game state, even parts that the player should not have access to.
/// - do actions that are invalid by standard game rules, like adding money.
/// </summary>
public class CheatingGameSessionController : GameSessionController
{
    public CheatingGameSessionController(GameSession gameSession) : base(gameSession)
    {
    }

    public GameState GameState()
        => GameSession.CurrentGameState;

    public void AddMoney(int count)
        => throw new NotImplementedException();
}