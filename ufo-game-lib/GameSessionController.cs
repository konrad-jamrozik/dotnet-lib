namespace UfoGameLib;

/// <summary>
/// Represents means for controlling GameSession, to be called by a client logic (e.g. CLI) acting on behalf of
/// a player, whether human or automated.
///
/// Provides following features over raw access to GameSession:
/// - Convenient methods representing player actions, that are translated by the controller
/// to underlying low-level GameSession method invocations.
/// - Restricted Read/Write access to the GameSession. Notably, a player should not be able
/// to read entire game session state, only parts visible to it.
/// </summary>
public class GameSessionController
{
    protected readonly GameSession GameSession;

    public GameSessionController(GameSession gameSession)
    {
        GameSession = gameSession;
    }

    public void AdvanceTime()
        => GameSession.ApplyPlayerActions(new AdvanceTimePlayerAction());

    public void HireAgents(int count)
        => GameSession.ApplyPlayerActions(new HireAgentsPlayerAction(count));

    public void FireAgents(IEnumerable<string> agentNames)
        => throw new NotImplementedException();

    public void LaunchMission(int agentCount)
        => GameSession.ApplyPlayerActions(new LaunchMissionPlayerAction(agentCount));
}