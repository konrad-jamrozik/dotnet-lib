using UfoGame.Model.Data;

namespace UfoGame.Model;

// kja rename MissionPrep to MissionDeployment
public class MissionPrep
{
    public readonly MissionPrepData Data;

    public int MinAgentsSendableOnMission => 1;

    public int MaxAgentsSendableOnMission => Math.Min(
        Data.TransportCapacity,
        _agents.AgentsSendableOnMissionCount);

    private readonly Agents _agents;

    public MissionPrep(MissionPrepData data, Agents agents)
    {
        Data = data;
        _agents = agents;
    }
}