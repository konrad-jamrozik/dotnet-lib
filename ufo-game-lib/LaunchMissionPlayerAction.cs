namespace UfoGameLib;

public class LaunchMissionPlayerAction : PlayerAction
{
    public int AgentCount { get; }

    public LaunchMissionPlayerAction(int agentCount)
    {
        AgentCount = agentCount;
    }

    public override void Apply(GameState gameState)
    {
        gameState.Archive.MissionsLaunchedCount += 1;
    }
}