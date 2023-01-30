using System.Text.Json.Serialization;
using UfoGame.Infra;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Agents : IResettable
{
    private readonly AgentsData _agentsData;
    private readonly TimelineData _timelineData;
    private readonly ArchiveData _archiveData;
    private readonly RandomGen _randomGen;

    private List<Agent> _data;

    public Agents(AgentsData agentsData, TimelineData timelineData, ArchiveData archiveData, RandomGen randomGen)
    {
        _agentsData = agentsData;
        _timelineData = timelineData;
        _archiveData = archiveData;
        _randomGen = randomGen;
        _data = AgentsFromData(_agentsData.Data).ToList();
    }

    [JsonIgnore]
    public List<Agent> AvailableAgents => _data.Where(agent => agent.Available).ToList();

    public List<Agent> AvailableAgentsSortedByLaunchPriority()
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
        => AvailableAgentsSortedByLaunchPriority()
            .Where(s => !s.Data.AssignedToMission)
            .ToList();

    public List<Agent> AssignedAgentsSortedByDescendingLaunchPriority(int currentTime)
        => AvailableAgentsSortedByLaunchPriority()
            .Where(agent => agent.Data.AssignedToMission)
            .Reverse()
            .ToList();

    public int AgentsAssignedToMissionCount => _data.Count(agent => agent.Data.AssignedToMission);

    [JsonIgnore]
    public List<Agent> AgentsAssignedToMission
        => _data.Where(agent => agent.Data.AssignedToMission).ToList();

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
        _archiveData.RecordHiredAgents(agentsToHire);
    }

    public void SackAgent(Agent agent)
    {
        agent.Sack();
        _archiveData.RecordSackedAgent();
    }

    public void LoseAgents(List<(Agent agent, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.missionSuccess));
        _archiveData.RecordLostAgents(agents.Count);
    }

    private IEnumerable<Agent> AgentsFromData(List<AgentData> agentsData)
        => agentsData.Select(data => new Agent(data, _timelineData));

    public void Reset()
    {
        _agentsData.Reset();
        _data = AgentsFromData(_agentsData.Data).ToList();
    }
}