namespace UfoGameLib.Tests;

public class PlayerSimulator
{
    public readonly GameSession GameSession = new GameSession();

    public void AdvanceTime()
        => GameSession.ApplyPlayerActions(new AdvanceTimePlayerAction());

    public void HireAgents(int count)
        => GameSession.ApplyPlayerActions(new HireAgentsPlayerAction(count));

    public void LaunchMission(int agentCount)
        => GameSession.ApplyPlayerActions(new LaunchMissionPlayerAction(agentCount));

    public void FireAgents(IEnumerable<string> agentNames)
    {
        throw new NotImplementedException();
    }
}