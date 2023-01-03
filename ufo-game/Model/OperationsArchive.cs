﻿namespace UfoGame.Model;

public class OperationsArchive
{
    public int MissionsLaunched { get; private set; } = 0;

    public int SuccessfulMissions { get; private set; } = 0;

    public int FailedMissions { get; private set; } = 0;

    public int IgnoredMissions { get; private set; } = 0;

    public int TotalSoldiersHired { get; private set; } = 0;

    public int SoldiersLost { get; private set; } = 0;

    public string LastMissionReport { get; private set; } = "No missions yet!";

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

    public void RecordLostSoldiers(int amount)
    {
        SoldiersLost += amount;
        Console.Out.WriteLine($"Recorded {amount} lost soldiers. Lost soldiers now at {SoldiersLost}.");
    }
}
