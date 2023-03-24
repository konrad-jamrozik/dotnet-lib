namespace UfoGameLib;

public class GameEngine
{
    public GameState NewInitialGameState()
    {
        return new GameState(new Archive());
    }

    public (GameState nextGameState, GameStateComputationLog log) ComputeNextGameState(
        GameState gameState,
        PlayerActions playerActions)
    {
        GameState modifiedGameState = playerActions.Apply(gameState);
        return (modifiedGameState, new GameStateComputationLog());
    }
}