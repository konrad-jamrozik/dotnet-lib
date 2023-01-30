using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class ArchiveData : IPersistable, IResettable
{
    public const string NoMissionsReport = "No missions yet!";

    [JsonInclude] public int MissionsLaunched { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int IgnoredMissions { get; private set; }
    // kja TotalGentsHired, Sacked, and Lost can be derived; doesn't have to be saved.
    // kja drop the "Total" prefix from AgentsHired and AgentsSacked
    [JsonInclude] public int TotalAgentsHired { get; private set; }
    [JsonInclude] public int TotalAgentsSacked { get; private set; }
    [JsonInclude] public int AgentsLost { get; private set; }
    [JsonInclude] public string LastMissionReport { get; private set; } = string.Empty;

    public ArchiveData()
        => Reset();

    public void ArchiveMission(bool missionSuccessful)
    {
        MissionsLaunched += 1;
        if (missionSuccessful)
            SuccessfulMissions += 1;
        else
            FailedMissions += 1;
        
        Console.Out.WriteLine($"Recorded {(missionSuccessful ? "successful" : "failed")} mission.");
    }

    public void WriteLastMissionReport(string missionReport)
    {
        LastMissionReport = missionReport;
    }

    public void RecordIgnoredMission()
    {
        IgnoredMissions += 1;
        Console.Out.WriteLine($"Recorded ignored mission. Total: {IgnoredMissions}.");
    }

    public void RecordHiredAgents(int count)
    {
        TotalAgentsHired += count;
        Console.Out.WriteLine($"Recorded hired {count} agents. Total: {TotalAgentsHired}.");
    }

    public void RecordSackedAgent()
    {
        TotalAgentsSacked += 1;
        Console.Out.WriteLine($"Recorded sacked agent. Total: {TotalAgentsSacked}.");
    }

    public void RecordLostAgents(int amount)
    {
        AgentsLost += amount;
        Console.Out.WriteLine($"Recorded {amount} lost agents. Lost agents now at {AgentsLost}.");
    }

    public void Reset()
    {
        MissionsLaunched = 0;
        SuccessfulMissions = 0;
        FailedMissions = 0;
        IgnoredMissions = 0;
        TotalAgentsHired = 0;
        TotalAgentsSacked = 0;
        AgentsLost = 0;
        LastMissionReport = NoMissionsReport;
    }
}
