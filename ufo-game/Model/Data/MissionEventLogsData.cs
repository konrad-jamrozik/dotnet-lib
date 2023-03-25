using System.Text.Json.Serialization;
using static UfoGame.Model.MissionOutcome;

namespace UfoGame.Model.Data;

// turned-off-to-do wip: stuff logged here should be added to the /mission page as well as to console.out
public class MissionEventLogsData : IPersistable, IResettable
{
    [JsonInclude] public List<MissionEventLogData> Data = new List<MissionEventLogData>();

    public void LogAgentsSent(Agents agents)
        => Add(summary: $"Sent {agents.AgentsAssignedToMission.Count} agents.");

    public void LogAgentOutcome(bool missionSuccessful, AgentOutcome agentOutcome)
    {
        float recovery = agentOutcome.Recovery(missionSuccessful);
        var messageSuffix = agentOutcome.Survived ? $" Need {recovery} units of recovery." : "";
        var inequalitySign = agentOutcome.Roll <= agentOutcome.SurvivalChance ? "<=" : ">";

        Add(
            summary: $"Agent #{agentOutcome.Agent.Data.Id} '{agentOutcome.Agent.Data.FullName}' " +
                     $"{(agentOutcome.Survived ? "survived" : "lost")}.",
            details: $"Agent exp: {agentOutcome.ExpBonus}. Agent rolled " +
                     $"{agentOutcome.Roll} {inequalitySign} {agentOutcome.SurvivalChance}." +
                     messageSuffix);
    }

    public void LogMissionReport(string missionEventSummary, string missionReport)
        => Add(summary: missionEventSummary, details: missionReport);

    public void Add(string summary, string? details = null)
        => Data.Add(new MissionEventLogData(summary, details));

    public void Reset()
        => Data = new List<MissionEventLogData>();
}