namespace UfoGameLib;

public class GameSession
{
    private readonly GameEngine _gameEngine;
    public readonly List<GameState> GameStates;
    public GameState CurrentGameState => GameStates.Last();

    public GameSession()
    {
        _gameEngine = new GameEngine();
        GameStates = new List<GameState> {_gameEngine.NewInitialGameState() };
    }

    public void ApplyPlayerActions(params PlayerAction[] actions)
    {
        PlayerActions playerActions = new PlayerActions(actions);
        (GameState nextGameState, GameStateComputationLog log) = _gameEngine.ComputeNextGameState(CurrentGameState, playerActions);
        GameStates.Add(nextGameState);
    }
}