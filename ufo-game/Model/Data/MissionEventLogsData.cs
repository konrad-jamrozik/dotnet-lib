using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionEventLogsData : IPersistable, IResettable
{
    [JsonInclude] public List<MissionEventLogData> Data = new List<MissionEventLogData>();

    public void LogAgentsSent(Agents agents)
        => Add(summary: $"Sent {agents.AgentsAssignedToMission.Count} agents.");

    public void LogMissionReport(string missionEventSummary, string missionReport)
        => Add(summary: missionEventSummary, details: missionReport);

    private void Add(string summary, string? details = null)
        => Data.Add(new MissionEventLogData(summary, details));

    public void Reset()
        => Data = new List<MissionEventLogData>();
}