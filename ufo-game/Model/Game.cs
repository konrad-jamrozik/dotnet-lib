using System.Diagnostics;

namespace UfoGame.Model;

public class Game
{
    public const int WinScore = 500;
    public const int LoseScore = 100;

    public int MoneyRaisedAmount = 50;
    public const int MoneyPerTurnAmount = 0;
    
    public int MoneyRaisingMethodsResearchCost = 100;
    public const int MoneyRaisingMethodsResearchCostIncrement = 10;

    public int SoldierEffectivenessResearchCost = 100;
    public const int SoldierEffectivenessResearchCostIncrement = 10;

    public int SoldierSurvivabilityResearchCost = 100;
    public const int SoldierSurvivabilityResearchCostIncrement = 10;

    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly OperationsArchive Archive;
    public readonly MissionPrep MissionPrep;
    public readonly PendingMission PendingMission;
    public readonly StateRefresh StateRefresh;
    public readonly Factions Factions;
    public readonly PlayerScore PlayerScore;

    public Game(Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive archive,
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        Factions factions,
        PlayerScore playerScore)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        Archive = archive;
        MissionPrep = missionPrep;
        PendingMission = pendingMission;
        StateRefresh = stateRefresh;
        Factions = factions;
        PlayerScore = playerScore;
    }

    public void DoNothing() => AdvanceTime();

    public void AdvanceTime(bool addMoney = true)
    {
        Timeline.IncrementTime();
        if (addMoney)
            Money.AddMoney(MoneyPerTurnAmount);
        PendingMission.AdvanceMissionTime();
        Factions.AdvanceFactionsTime();
        StateRefresh.Trigger();
    }

    public void RaiseMoney()
    {
        Money.AddMoney(MoneyRaisedAmount);
        AdvanceTime(addMoney: false);
    }

    public bool CanResearchMoneyRaisingMethods()
        => Money.CurrentMoney >= MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Money.SubtractMoney(MoneyRaisingMethodsResearchCost);
        MoneyRaisingMethodsResearchCost += MoneyRaisingMethodsResearchCostIncrement;
        MoneyRaisedAmount += 5;
        AdvanceTime();
    }

    public bool CanResearchSoldierEffectiveness()
        => Money.CurrentMoney >= SoldierEffectivenessResearchCost;

    public void ResearchSoldierEffectiveness()
    {
        Debug.Assert(CanResearchSoldierEffectiveness());
        Money.SubtractMoney(SoldierEffectivenessResearchCost);
        SoldierEffectivenessResearchCost += SoldierEffectivenessResearchCostIncrement;
        Staff.SoldierEffectiveness += 10;
        AdvanceTime();
    }

    public bool CanResearchSoldierSurvivability()
        => Money.CurrentMoney >= SoldierSurvivabilityResearchCost;

    public void ResearchSoldierSurvivability()
    {
        Debug.Assert(CanResearchSoldierSurvivability());
        Money.SubtractMoney(SoldierSurvivabilityResearchCost);
        SoldierSurvivabilityResearchCost += SoldierSurvivabilityResearchCostIncrement;
        Staff.SoldierSurvivability += 10;
        AdvanceTime();
    }

    public void HireSoldier()
    {
        Debug.Assert(CanHireSoldier());
        Money.SubtractMoney(Staff.SoldierPrice);
        Staff.HireSoldier();
        Archive.RecordHiredSoldier();
        StateRefresh.Trigger();
    }

    public bool CanHireSoldier()
    {
        return Money.CurrentMoney >= Staff.SoldierPrice;
    }

    public void LaunchMission()
    {
        // Roll between 1 and 100.
        // The lower the better.
        int roll = new Random().Next(100) + 1;
        bool success = roll <= PendingMission.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {PendingMission.SuccessChance} resulting in {(success ? "success" : "failure")}");

        if (success)
        {
            // kja what if enemy had less than WinScore left? Does all of it go to player? 
            // Some other bonus for finishing off an enemy faction?
            PlayerScore.Value += WinScore;
            PendingMission.Faction.Score -= WinScore;
        }
        else
        {
            // kja need to end the game when player score reaches zero;
            // some pop-up + grey out all buttons, like "advance time" and "launch mission" ?
            PlayerScore.Value -= LoseScore;
            PendingMission.Faction.Score += LoseScore;
        }

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
        string missionSuccessReport = success 
            ? $"successful! We took {WinScore} score from {PendingMission.Faction.Name}" 
            : $"a failure. We lost {LoseScore} score to {PendingMission.Faction.Name}.";
        string missionRollReport =
            $" (Rolled {roll} against limit of {PendingMission.SuccessChance}.)";
        string soldiersLostReport = soldiersLost > 0 ? $"Number of soldiers lost: {soldiersLost}." : "We didn't lose any soldiers.";
        Archive.WriteLastMissionReport($"The last mission was {missionSuccessReport} {missionRollReport} {soldiersLostReport}");
        PendingMission.GenerateNew();
        StateRefresh.Trigger();
    }

    public bool CanLaunchMission()
    {
        Console.Out.WriteLine("Can launch mission?");
        return PendingMission.CurrentlyAvailable 
               && MissionPrep.SoldiersToSend >= 1
               && MissionPrep.SoldiersToSend <= Staff.CurrentSoldiers;
    }
}
