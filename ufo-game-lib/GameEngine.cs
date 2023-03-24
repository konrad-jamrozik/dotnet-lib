namespace UfoGameLib;

public class GameEngine
{
    public GameState NewInitialGameState()
    {
        return new GameState(new Timeline(CurrentTurn: 0), new Archive());
    }

    public (GameState nextGameState, GameStateComputationLog log) ComputeNextGameState(
        GameState gameState,
        PlayerActions playerActions)
    {
        GameState modifiedGameState = playerActions.Apply(gameState);
        modifiedGameState.Timeline.CurrentTurn++;
        return (modifiedGameState, new GameStateComputationLog());
    }
}