namespace UfoGameLib;

public class GameSessionController
{
    private readonly GameSession _gameSession;

    public GameSessionController(GameSession gameSession)
    {
        _gameSession = gameSession;
    }

    public void AdvanceTime()
        => _gameSession.ApplyPlayerActions(new AdvanceTimePlayerAction());

    public void HireAgents(int count)
        => _gameSession.ApplyPlayerActions(new HireAgentsPlayerAction(count));

    public void FireAgents(IEnumerable<string> agentNames)
        => throw new NotImplementedException();

    public void LaunchMission(int agentCount)
        => _gameSession.ApplyPlayerActions(new LaunchMissionPlayerAction(agentCount));
}