using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class LaunchMissionPlayerAction : PlayerAction
{
    private readonly Mission _mission;
    public int AgentCount { get; }

    public LaunchMissionPlayerAction(Mission mission, int agentCount)
    {
        _mission = mission;
        AgentCount = agentCount;
    }

    public override void Apply(GameState state)
    {
        Console.Out.WriteLine($"LaunchMissionPlayerAction.Apply");
        // kja NEXT use mission passed as param.
        // The problem right now is that I am using "Mission" both as "Mission site pending deployment"
        // as well as "Mission in progress"
        // kja need to decrease TransportCapacity by the agents sent until mission is completed (for now it just means time is advanced)
        state.Missions.Add(new Mission(state.NextMissionId));
    }
}