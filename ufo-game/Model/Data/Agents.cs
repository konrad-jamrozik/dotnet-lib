using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class Agents
{
    private readonly AgentsData _agentsData;
    private readonly TimelineData _timelineData;
    private readonly SickBay _sickBay;
    private readonly Archive _archive;
    private List<Agent> _data;

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

    public void LoseAgents(List<(Agent agent, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.missionSuccess));
        _archive.RecordLostAgents(agents.Count);
    }

    public Agents(AgentsData agentsData, TimelineData timelineData, SickBay sickBay, Archive archive)
    {
        _agentsData = agentsData;
        _timelineData = timelineData;
        _sickBay = sickBay;
        _archive = archive;
        _data = AgentsFromData(_agentsData.Data).ToList();
    }
    public void Reset()
    {
        _agentsData.Reset();
        _data = AgentsFromData(_agentsData.Data).ToList();
    }

    // kja move this to sick bay; requires extracting SickBayData
    public void AdvanceTime()
        => AgentsInRecovery.ForEach(agent => agent.TickRecovery(_sickBay.Data.AgentRecoverySpeed));

    public void AddNewRandomAgents(int agentsToAdd)
    {
        var addedAgentsData = _agentsData.AddNewRandomAgents(agentsToAdd, _timelineData.CurrentTime);
        _data.AddRange(AgentsFromData(addedAgentsData));
    }

    private IEnumerable<Agent> AgentsFromData(List<AgentData> agentsData)
        => agentsData.Select(data => new Agent(data, _timelineData));
}