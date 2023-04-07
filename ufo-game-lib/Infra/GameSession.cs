using System.Runtime.CompilerServices;
using UfoGameLib.Model;

namespace UfoGameLib.Infra;

/// <summary>
/// GameSession represent an instance of a game session (playthrough).
///
/// As such, it maintains a reference to current GameState as well as few most recent states for
/// limited undo capability.
///
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
        if (GameStates.Count > 10)
            GameStates.RemoveAt(0);
        Debug.Assert(GameStates.Count <= 10);
    }

    private static (GameState updatedState, GameStateUpdateLog log) UpdateGameState(
        GameState state,
        PlayerActions actions)
    {
        Debug.Assert(!state.IsGameOver);
        Debug.Assert(!state.IsPast);
        GameState updatedState = state with { Id = state.Id + 1 };
        state.IsPast = true;
        actions.Apply(updatedState);
        return (updatedState, new GameStateUpdateLog());
    }
}