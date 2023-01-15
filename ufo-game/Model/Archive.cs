﻿using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Archive
{
    public const string NoMissionsReport = "No missions yet!";

    [JsonInclude] public int MissionsLaunched { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int IgnoredMissions { get; private set; }
    [JsonInclude] public int TotalSoldiersHired { get; private set; }
    [JsonInclude] public int TotalSoldiersFired { get; private set; }
    [JsonInclude] public int SoldiersLost { get; private set; }
    [JsonInclude] public string LastMissionReport { get; private set; } = string.Empty;

    public Archive()
        => Reset();

    public void Reset()
    {
        MissionsLaunched = 0;
        SuccessfulMissions = 0;
        FailedMissions = 0;
        IgnoredMissions = 0;
        TotalSoldiersHired = 0;
        TotalSoldiersFired = 0;
        SoldiersLost = 0;
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

    public void RecordHiredSoldiers(int count)
    {
        TotalSoldiersHired += count;
        Console.Out.WriteLine($"Recorded hired {count} soldiers. Total: {TotalSoldiersHired}.");
    }

    public void RecordFiredSoldiers(int count)
    {
        TotalSoldiersFired += count;
        Console.Out.WriteLine($"Recorded fired {count} soldiers. Total: {TotalSoldiersFired}.");
    }

    public void RecordLostSoldiers(int amount)
    {
        SoldiersLost += amount;
        Console.Out.WriteLine($"Recorded {amount} lost soldiers. Lost soldiers now at {SoldiersLost}.");
    }
}