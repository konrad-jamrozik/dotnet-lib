using System.Text.Json.Serialization;
using UfoGame.Infra;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Agents : IResettable
{
    private readonly AgentsData _agentsData;
    private readonly TimelineData _timelineData;
    private readonly RandomGen _randomGen;

    private List<Agent> _data;

    public Agents(AgentsData agentsData, TimelineData timelineData, RandomGen randomGen)
    {
        _agentsData = agentsData;
        _timelineData = timelineData;
        _randomGen = randomGen;
        _data = AgentsFromData(_agentsData.Data).ToList();
    }

    public int HiredCount => _data.Count;
    public int SackedCount => _data.Count(agent => agent.Sacked);
    public int LostCount => _data.Count(agent => agent.Lost);

    [JsonIgnore]
    public List<Agent> AvailableAgents => _data.Where(agent => agent.Available).ToList();

    public List<Agent> AgentsSendableOnMissionSortedByLaunchPriority()
        => AvailableAgents
            // First list agents that can be sent on a mission.
            .OrderByDescending(agent => agent.CanSendOnMission)
            // Then sort by recovery - all agents that can be sent have recovery 0,
            // but those who cannot will be sorted in increasing remaining recovery need.
            .ThenBy(agent => agent.Data.Recovery)
            // Then sort agents by exp ascending
            // (rookies to be sent first, hence listed  first).
            .ThenBy(agent => agent.ExperienceBonus())
            // Then from agents of the same experience bonus, first list
            // the ones hired more recently.
            .ThenByDescending(agent => agent.Data.Id)
            .ToList();

    public List<Agent> AssignableAgentsSortedByLaunchPriority(int currentTime)
        => AgentsSendableOnMissionSortedByLaunchPriority()
            .Where(s => !s.Data.AssignedToMission)
            .ToList();

    public List<Agent> AssignedAgentsSortedByDescendingLaunchPriority(int currentTime)
        => AgentsSendableOnMissionSortedByLaunchPriority()
            .Where(agent => agent.Data.AssignedToMission)
            .Reverse()
            .ToList();

    public int AgentsAssignedToMissionCount => _data.Count(agent => agent.Data.AssignedToMission);

    [JsonIgnore]
    public List<Agent> AgentsAssignedToMission
        => _data.Where(agent => agent.Data.AssignedToMission).ToList();

    [JsonIgnore]
    public List<Agent> AgentsAssignableToMission
        => _data.Where(agent => agent.IsAssignableToMission).ToList();

    [JsonIgnore]
    public List<Agent> AgentsInRecovery
        => _data.Where(agent => agent.IsRecovering).ToList();

    [JsonIgnore]
    public int AgentsInRecoveryCount
        => _data.Count(agent => agent.IsRecovering);

    public int AgentsSendableOnMissionCount
        => _data.Count(agent => agent.CanSendOnMission);

    public void HireAgents(int agentsToHire)
    {
        var addedAgentsData = _agentsData.AddNewRandomAgents(agentsToHire, _timelineData.CurrentTime, _randomGen);
        _data.AddRange(AgentsFromData(addedAgentsData));
    }

    public void SackAgent(Agent agent)
    {
        agent.Sack();
    }

    public void LoseAgents(List<(Agent agent, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.missionSuccess));
    }

    private IEnumerable<Agent> AgentsFromData(List<AgentData> agentsData)
        => agentsData.Select(data => new Agent(data, _timelineData));

    public void Reset()
    {
        _agentsData.Reset();
        _data = AgentsFromData(_agentsData.Data).ToList();
    }
}