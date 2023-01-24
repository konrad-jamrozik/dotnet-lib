using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class ArchiveData
{
    public const string NoMissionsReport = "No missions yet!";

    [JsonInclude] public int MissionsLaunched { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int IgnoredMissions { get; private set; }
    [JsonInclude] public int TotalAgentsHired { get; private set; }
    [JsonInclude] public int TotalAgentsFired { get; private set; }
    [JsonInclude] public int AgentsLost { get; private set; }
    [JsonInclude] public string LastMissionReport { get; private set; } = string.Empty;

    public ArchiveData()
        => Reset();

    public void Reset()
    {
        MissionsLaunched = 0;
        SuccessfulMissions = 0;
        FailedMissions = 0;
        IgnoredMissions = 0;
        TotalAgentsHired = 0;
        TotalAgentsFired = 0;
        AgentsLost = 0;
        LastMissionReport = NoMissionsReport;
    }

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

    public void RecordFiredAgents(int count)
    {
        TotalAgentsFired += count;
        Console.Out.WriteLine($"Recorded fired {count} agents. Total: {TotalAgentsFired}.");
    }

    public void RecordLostAgents(int amount)
    {
        AgentsLost += amount;
        Console.Out.WriteLine($"Recorded {amount} lost agents. Lost agents now at {AgentsLost}.");
    }
}
