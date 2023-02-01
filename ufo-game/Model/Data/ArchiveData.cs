using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class ArchiveData : IPersistable, IResettable
{
    public const string NoMissionsReport = "No missions yet!";

    [JsonInclude] public int MissionsLaunched { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int IgnoredMissions { get; private set; }
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

    public void Reset()
    {
        MissionsLaunched = 0;
        SuccessfulMissions = 0;
        FailedMissions = 0;
        IgnoredMissions = 0;
        LastMissionReport = NoMissionsReport;
    }
}
