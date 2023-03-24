namespace UfoGameLib;

public class LaunchMissionPlayerAction : PlayerAction
{
    public int AgentsCount { get; }

    public LaunchMissionPlayerAction(int agentsCount)
    {
        AgentsCount = agentsCount;
    }
}