namespace UfoGameLib.Infra;

/// <summary>
/// GameSession represent an instance of a game session (playthrough).
///
/// As such, it maintains a reference to current GameState as well as few most recent states for limited undo capability.
/// In addition, it allows updating of the game state by applying PlayerActions.
///
/// GameSession must be accessed directly only by GameSessionController.
/// </summary>
public class GameSession
{
    public readonly List<GameState> GameStates = new List<GameState> { GameState.NewInitialGameState() };

    public GameState CurrentGameState => GameStates.Last();

    public void ApplyPlayerActions(params PlayerAction[] actionsData)
    {
        PlayerActions actions = new PlayerActions(actionsData);
        (GameState updatedState, GameStateUpdateLog log) = UpdateGameState(CurrentGameState, actions);

        GameStates.Add(updatedState);

        // Keep only the most recent game states to avoid eating too much memory.
        // Consider that every GameState keeps track of all missions, so
        // the space usage grows O(state_count * mission_count). Similar
        // with agents.
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