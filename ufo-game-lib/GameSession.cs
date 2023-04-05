namespace UfoGameLib;

public class GameSession
{
    public readonly List<GameState> GameStates = new List<GameState> { GameState.NewInitialGameState() };

    // kja this should have restricted access, because Player should not be allowed to see entire GameState. Only "player visible game state".
    // This probably should be fixed by introducing an abstraction on top of GameSession.
    // That abstraction:
    // - Would have the convenience methods, like AdvanceTime() or HireAgents()
    // - Would restrict access to GameState to only player-visible parts
    // - Player instance could be created only with instance of that type, not of GameSession.
    // How to name it?
    // - GameSessionController, PlayerGameSession, GameSessionPlayerFacade, GameSessionPlayerInterface,
    // - I think GameSessionController
    // See also comment in GameSessionTests
    public GameState CurrentGameState => GameStates.Last();

    public void FireAgents(IEnumerable<string> agentNames)
    {
        throw new NotImplementedException();
    }

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