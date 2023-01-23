using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class StaffData
{
    public const float AgentRecoverySpeedImprovement = 0.25f;

    [JsonInclude] public int NextAgentId;
    [JsonInclude] public int AgentEffectiveness;
    [JsonInclude] public int AgentSurvivability;
    [JsonInclude] public float AgentRecoverySpeed { get; private set; }
    [JsonInclude] public List<Agent> Agents = new List<Agent>();

    [JsonIgnore]
    public List<Agent> AvailableAgents => Agents.Where(s => s.Available).ToList();

    public List<Agent> AvailableAgentsSortedByLaunchPriority(int currentTime)
        => AvailableAgents
            // First list agents that can be sent on a mission.
            .OrderByDescending(s => s.CanSendOnMission)
            // Then sort by recovery - all agents that can be sent have recovery 0,
            // but those who cannot will be sorted in increasing remaining recovery need.
            .ThenBy(s => s.Recovery)
            // Then sort agents by exp ascending
            // (rookies to be sent first, hence listed  first).
            .ThenBy(s => s.ExperienceBonus(currentTime))
            // Then from agents of the same experience bonus, first list
            // the ones hired more recently.
            .ThenByDescending(s => s.Id)
            .ToList();

    public List<Agent> AssignableAgentsSortedByLaunchPriority(int currentTime)
        => AvailableAgentsSortedByLaunchPriority(currentTime)
            .Where(s => !s.AssignedToMission)
            .ToList();

    public List<Agent> AssignedAgentsSortedByDescendingLaunchPriority(int currentTime)
        => AvailableAgentsSortedByLaunchPriority(currentTime)
            .Where(s => s.AssignedToMission)
            .Reverse()
            .ToList();

    public int AgentsAssignedToMissionCount => Agents.Count(s => s.AssignedToMission);

    [JsonIgnore]
    public List<Agent> AgentsAssignedToMission
        => Agents.Where(s => s.AssignedToMission).ToList();

    [JsonIgnore]
    public List<Agent> AgentsInRecovery
        => Agents.Where(s => s.IsRecovering).ToList();

    [JsonIgnore]
    public int AgentsInRecoveryCount
        => Agents.Count(s => s.IsRecovering);


    public int AgentsSendableOnMissionCount
        => Agents.Count(s => s.CanSendOnMission);

    public void ImproveAgentRecoverySpeed()
        => AgentRecoverySpeed += AgentRecoverySpeedImprovement;

    public StaffData()
        => Reset();

    public void Reset()
    {
        NextAgentId = 0;
        AgentEffectiveness = 100;
        AgentSurvivability = 100;
        AgentRecoverySpeed = 0.5f;
        Agents = new List<Agent>();
    }

    public void AdvanceTime()
    {
        AgentsInRecovery.ForEach(s => s.TickRecovery(AgentRecoverySpeed));
    }

    public void AddNewRandomAgents(int agentsToAdd, int currentTime)
    {
        Enumerable.Range(NextAgentId, agentsToAdd)
            .ToList()
            .ForEach(
                id => Agents.Add(
                    new Agent(
                        id,
                        AgentNames.RandomName(),
                        currentTime)));
        NextAgentId += agentsToAdd;
        Console.Out.WriteLine($"Added {agentsToAdd} new, random agents. Available agents now at {AvailableAgents.Count}.");
    }
}