namespace UfoGameLib;

public class GameEngine
{
    public GameState NewInitialGameState()
    {
        throw new NotImplementedException();
    }

    public (GameState nextGameState, GameStateComputationLog log) ComputeNextGameState(
        GameState gameState,
        PlayerActions playerActions)
    {
        throw new NotImplementedException();
    }
}