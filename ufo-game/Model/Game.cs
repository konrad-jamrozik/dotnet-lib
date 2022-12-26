namespace UfoGame.Model;

public class Game
{
    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly OperationsArchive Archive;
    public readonly MissionPrep MissionPrep;
    public readonly PendingMission PendingMission;

    public Game(Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive archive,
        MissionPrep missionPrep,
        PendingMission pendingMission)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        Archive = archive;
        MissionPrep = missionPrep;
        PendingMission = pendingMission;
    }

    public void AdvanceTime()
    {
        Timeline.IncrementTime();
        Money.AddMoney(10);
        PendingMission.RandomizeDifficulty();
    }

    public void HireSoldier()
    {
        Money.SubtractMoney(Staff.SoldierPrice);
        Staff.HireSoldier();
    }

    public bool CanHireSoldier()
    {
        return Money.CurrentMoney >= Staff.SoldierPrice;
    }

    public void LaunchMission()
    {
        // Roll between 1 and 100.
        // The lower the better.
        int roll = new Random().Next(101);
        bool success = roll <= PendingMission.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {PendingMission.SuccessChance} resulting in {(success ? "success" : "failure")}");

        // If success, lose 0-50% soldiers, 50% rounded down.
        // If failure, lose 50-100% soldiers, 50% rounded down.
        int minSoldiersLost = success ? 0 : MissionPrep.SoldiersToSend / 2;
        int maxSoldiersLost = success ? MissionPrep.SoldiersToSend / 2 : MissionPrep.SoldiersToSend;
        
        int soldiersLost = new Random().Next(minSoldiersLost, maxSoldiersLost + 1);

        if (soldiersLost > 0)
        {
            Archive.RecordLostSoldiers(soldiersLost);
            Staff.SubtractSoldiers(soldiersLost);
        }
        else
        {
            Console.Out.WriteLine("No soldiers lost! \\o/");
        }

        Archive.ArchiveMission(missionSuccessful: success);
        PendingMission.RandomizeDifficulty();
    }

    public bool CanLaunchMission()
    {
        return MissionPrep.SoldiersToSend >= 1 &&
               MissionPrep.SoldiersToSend <= Staff.CurrentSoldiers;
    }
}
