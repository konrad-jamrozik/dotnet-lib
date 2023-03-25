namespace UfoGameLib;

public class GameSession
{
    public readonly List<GameState> GameStates = new List<GameState> { GameState.NewInitialGameState() };
    public GameState CurrentGameState => GameStates.Last();

    public void ApplyPlayerActions(params PlayerAction[] actionsData)
    {
        PlayerActions actions = new PlayerActions(actionsData);
        (GameState updatedState, GameStateUpdateLog log) = UpdateGameState(CurrentGameState, actions);

        GameStates.Add(updatedState);

        // Keep only the most recent game states.
        if (GameStates.Count > 5)
            GameStates.RemoveAt(0);
        Debug.Assert(GameStates.Count <= 5);
    }

    private (GameState updatedState, GameStateUpdateLog log) UpdateGameState(
        GameState state,
        PlayerActions actions)
    {
        Debug.Assert(!state.GameOver);
        GameState updatedState = state with { };
        actions.Apply(updatedState);
        return (updatedState, new GameStateUpdateLog());
    }
}