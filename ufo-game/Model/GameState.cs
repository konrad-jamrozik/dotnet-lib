namespace UfoGame.Model;

public class GameState
{
    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly OperationsArchive Archive;

    public GameState(Timeline timeline, Money money, Staff staff, OperationsArchive archive)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        Archive = archive;
    }

    public void AdvanceTime()
    {
        Timeline.IncrementTime();
        Money.AddMoney(10);
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
        int roll = new Random().Next(100) + 1;
        int successPercentage = 60;
        int successfulThreshold = 100 - successPercentage;
        bool successful = roll >= successfulThreshold;
        Archive.ArchiveMission(missionSuccessful: successful);
    }

    public bool CanLaunchMission()
    {
        return Staff.CurrentSoldiers > 0;
    }
}
