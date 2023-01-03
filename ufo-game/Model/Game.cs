using System.Diagnostics;

namespace UfoGame.Model;

public class Game
{
    public int MoneyRaisedAmount = 50;
    // Currently zero, as it offsets costs of actions, resulting in confusing
    // balance.
    public const int MoneyPerTurnAmount = 0;
    
    public int MoneyRaisingMethodsResearchCost = 100;
    public const int MoneyRaisingMethodsResearchCostIncrement = 10;

    public int SoldierEffectivenessResearchCost = 100;
    public const int SoldierEffectivenessResearchCostIncrement = 10;

    public int SoldierSurvivabilityResearchCost = 100;
    public const int SoldierSurvivabilityResearchCostIncrement = 10;

    private readonly Random _random = new Random();
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

    public bool CanDoNothing() => !PlayerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public void AdvanceTime(bool addMoney = true)
    {
        Debug.Assert(!PlayerScore.GameOver);
        Timeline.IncrementTime();
        if (addMoney)
            Money.AddMoney(MoneyPerTurnAmount);
        PendingMission.AdvanceMissionTime();
        Factions.AdvanceFactionsTime();
        StateRefresh.Trigger();
    }

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
    {
        Money.AddMoney(MoneyRaisedAmount);
        AdvanceTime(addMoney: false);
    }

    public bool CanResearchMoneyRaisingMethods()
        => !PlayerScore.GameOver && Money.CurrentMoney >= MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Money.SubtractMoney(MoneyRaisingMethodsResearchCost);
        MoneyRaisingMethodsResearchCost += MoneyRaisingMethodsResearchCostIncrement;
        MoneyRaisedAmount += 5;
        AdvanceTime();
    }

    public bool CanResearchSoldierEffectiveness()
        => !PlayerScore.GameOver && Money.CurrentMoney >= SoldierEffectivenessResearchCost;

    public void ResearchSoldierEffectiveness()
    {
        Debug.Assert(CanResearchSoldierEffectiveness());
        Money.SubtractMoney(SoldierEffectivenessResearchCost);
        SoldierEffectivenessResearchCost += SoldierEffectivenessResearchCostIncrement;
        Staff.SoldierEffectiveness += 10;
        AdvanceTime();
    }

    public bool CanResearchSoldierSurvivability()
        => !PlayerScore.GameOver && Money.CurrentMoney >= SoldierSurvivabilityResearchCost;

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
        Staff.HireSoldiers();
        StateRefresh.Trigger();
    }

    // kja this should be in PendingMission
    public void LaunchMission()
    {
        Debug.Assert(PendingMission.CanLaunchMission());
        // Roll between 1 and 100.
        // The lower the better.
        int roll = _random.Next(1, 100+1);
        bool success = roll <= PendingMission.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {PendingMission.SuccessChance} resulting in {(success ? "success" : "failure")}");

        int scoreDiff;
        if (success)
        {
            scoreDiff = Math.Min(PlayerScore.WinScore, PendingMission.Faction.Score);
            PlayerScore.Value += scoreDiff;
            PendingMission.Faction.Score -= scoreDiff;
            Money.AddMoney(PendingMission.MoneyReward);
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            PlayerScore.Value -= scoreDiff;
            PendingMission.Faction.Score += scoreDiff;
        }

        int soldiersLost = 0;
        for (int i = 0; i < MissionPrep.SoldiersToSend; i++)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int soldierRoll = _random.Next(1, 100+1);
            bool soldierSurvived = soldierRoll <= PendingMission.SoldierSurvivalChance;
            Console.Out.WriteLine(
                $"Soldier {i} {(soldierSurvived ? "survived" : "lost")}. " +
                $"Rolled {soldierRoll} <= {PendingMission.SoldierSurvivalChance}");
            if (!soldierSurvived)
                soldiersLost++;
        }

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
        string missionRollReport =
            $" (Rolled {roll} against limit of {PendingMission.SuccessChance}.)";
        string missionSuccessReport = success 
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {PendingMission.Faction.Name} and earned ${PendingMission.MoneyReward}." 
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {PendingMission.Faction.Name}.";
        
        string soldiersLostReport = soldiersLost > 0 ? $"Number of soldiers lost: {soldiersLost}." : "We didn't lose any soldiers.";
        Archive.WriteLastMissionReport($"The last mission was {missionSuccessReport} {soldiersLostReport}");
        PendingMission.GenerateNewOrClearMission();
        StateRefresh.Trigger();
    }
}
