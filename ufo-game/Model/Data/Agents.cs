using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class Agents
{
    private readonly AgentsData _agentsData;
    private readonly TimelineData _timelineData;
    private readonly SickBay _sickBay;
    public List<Agent> Data;

    [JsonIgnore]
    public List<Agent> AvailableAgents => Data.Where(agent => agent.Available).ToList();

    public List<Agent> AvailableAgentsSortedByLaunchPriority(int currentTime)
        => AvailableAgents
            // First list agents that can be sent on a mission.
            .OrderByDescending(agent => agent.CanSendOnMission)
            // Then sort by recovery - all agents that can be sent have recovery 0,
            // but those who cannot will be sorted in increasing remaining recovery need.
            .ThenBy(agent => agent.Data.Recovery)
            // Then sort agents by exp ascending
            // (rookies to be sent first, hence listed  first).
            .ThenBy(agent => agent.ExperienceBonus(currentTime))
            // Then from agents of the same experience bonus, first list
            // the ones hired more recently.
            .ThenByDescending(agent => agent.Data.Id)
            .ToList();

    public List<Agent> AssignableAgentsSortedByLaunchPriority(int currentTime)
        => AvailableAgentsSortedByLaunchPriority(currentTime)
            .Where(s => !s.Data.AssignedToMission)
            .ToList();

    public List<Agent> AssignedAgentsSortedByDescendingLaunchPriority(int currentTime)
        => AvailableAgentsSortedByLaunchPriority(currentTime)
            .Where(agent => agent.Data.AssignedToMission)
            .Reverse()
            .ToList();

    public int AgentsAssignedToMissionCount => Data.Count(agent => agent.Data.AssignedToMission);

    [JsonIgnore]
    public List<Agent> AgentsAssignedToMission
        => Data.Where(agent => agent.Data.AssignedToMission).ToList();

    [JsonIgnore]
    public List<Agent> AgentsInRecovery
        => Data.Where(agent => agent.IsRecovering).ToList();

    [JsonIgnore]
    public int AgentsInRecoveryCount
        => Data.Count(agent => agent.IsRecovering);

    public int AgentsSendableOnMissionCount
        => Data.Count(agent => agent.CanSendOnMission);

    public Agents(AgentsData agentsData, TimelineData timelineData, SickBay sickBay)
    {
        _agentsData = agentsData;
        _timelineData = timelineData;
        _sickBay = sickBay;
        Data = AgentsFromData(_agentsData.Data).ToList();
    }
    public void Reset()
    {
        _agentsData.Reset();
        Data = AgentsFromData(_agentsData.Data).ToList();
    }

    // kja move this to sick bay; requires extracting SickBayData
    public void AdvanceTime()
        => AgentsInRecovery.ForEach(agent => agent.TickRecovery(_sickBay.AgentRecoverySpeed));

    public void AddNewRandomAgents(int agentsToAdd)
    {
        var addedAgentsData = _agentsData.AddNewRandomAgents(agentsToAdd, _timelineData.CurrentTime);
        Data.AddRange(AgentsFromData(addedAgentsData));
    }

    private IEnumerable<Agent> AgentsFromData(List<AgentData> agentsData)
        => agentsData.Select(data => new Agent(data, _timelineData));
}