using UfoGameLib.Model;

namespace UfoGameLib.Infra;

/// <summary>
/// Represents means for controlling GameSession, to be called by a client logic (e.g. CLI) acting on behalf of
/// a player, whether human or automated.
///
/// Provides following features over raw access to GameSession:
/// - Convenient methods representing player actions, that are translated by the controller
/// to underlying low-level GameSession method invocations.
/// - Restricted Read/Write access to the GameSession. Notably, a player should not be able
/// to read entire game session state, only parts visible to it.
///
/// Here are few scenarios of using GameSessionController:
///
/// 1. A human player calls the CLI executable built from ufo-game-cli.
/// The implementation of that interface, Program.cs, translates the CLI commands to invocations
/// on GameSessionController which then returns the output to the human.
/// 2. As 1. but the CLI commands are called not by a human, but by an automated process.
/// 3. The CLI executable is used (by human (1.) or automated process (2.)) to launch a game session using an AI player.
/// As a result, the CLI programs ends up instantiating AIPlayer instance which then plays through the game by
/// invoking methods on GameSessionController.
///
/// The scenarios above can be visualized as follows, where "--" should be read as:
///   "Left side invokes right side, and right side returns output to the left side".
///
/// ```
/// 1. Human player       -- CLI executable -- Program --             GameSessionController -- GameSession
/// 2. Aut. proc. player  -- CLI executable -- Program --             GameSessionController -- GameSession
/// 3. Hum. or aut. proc. -- CLI executable -- Program -- AIPlayer -- GameSessionController -- GameSession
/// ```
/// </summary>
public class GameSessionController
{
    protected readonly GameSession GameSession;

    public GameSessionController(GameSession gameSession)
    {
        GameSession = gameSession;
    }

    public GameStatePlayerView GameStatePlayerView => new GameStatePlayerView(GameSession);

    public void AdvanceTime()
        => GameSession.ApplyPlayerActions(new AdvanceTimePlayerAction());

    public void HireAgents(int count)
        => GameSession.ApplyPlayerActions(new HireAgentsPlayerAction(count));

    public void FireAgents(IEnumerable<string> agentNames)
        => throw new NotImplementedException();

    public void LaunchMission(MissionSite site, int agentCount)
        => GameSession.ApplyPlayerActions(new LaunchMissionPlayerAction(site, agentCount));

    public void Save()
    {
        GameSession.CurrentGameState.Save();
    }

    public void Load()
    {
    }
}