using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Game
{
    [JsonInclude]
    public readonly Timeline Timeline;
    [JsonInclude]
    public readonly Money Money;
    [JsonInclude]
    public readonly Staff Staff;
    [JsonInclude]
    public readonly OperationsArchive OperationsArchive;
    [JsonInclude]
    public readonly MissionPrep MissionPrep;
    [JsonInclude]
    public readonly PendingMission PendingMission;
    [JsonInclude]
    public readonly Factions Factions;
    [JsonInclude]
    public readonly PlayerScore PlayerScore;
    [JsonInclude]
    public readonly Research Research;

    public readonly StateRefresh StateRefresh;
    public readonly PersistentStorage Storage;

    public Game(
        Timeline timeline,
        Money money,
        Staff staff,
        OperationsArchive operationsArchive,
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        Factions factions,
        PlayerScore playerScore,
        PersistentStorage storage,
        Research research)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
        OperationsArchive = operationsArchive;
        MissionPrep = missionPrep;
        PendingMission = pendingMission;
        StateRefresh = stateRefresh;
        Factions = factions;
        PlayerScore = playerScore;
        Storage = storage;
        Research = research;
    }

    public bool CanDoNothing() => !PlayerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public void AdvanceTime(bool addMoney = true)
    {
        Debug.Assert(!PlayerScore.GameOver);
        Timeline.IncrementTime();
        if (addMoney)
            Money.AddMoney(Money.MoneyPerTurnAmount);
        PendingMission.AdvanceMissionTime();
        Factions.AdvanceFactionsTime();
        StateRefresh.Trigger();
        // kja experimental
        Storage.PersistGameState(this);

    }

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
    {
        Money.AddMoney(Money.MoneyRaisedPerActionAmount);
        AdvanceTime(addMoney: false);
    }

    public bool CanResearchMoneyRaisingMethods()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Money.SubtractMoney(Research.MoneyRaisingMethodsResearchCost);
        Research.MoneyRaisingMethodsResearchCost += Research.MoneyRaisingMethodsResearchCostIncrement;
        Money.MoneyRaisedPerActionAmount += 5;
        AdvanceTime();
    }

    public bool CanResearchSoldierEffectiveness()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.SoldierEffectivenessResearchCost;

    public void ResearchSoldierEffectiveness()
    {
        Debug.Assert(CanResearchSoldierEffectiveness());
        Money.SubtractMoney(Research.SoldierEffectivenessResearchCost);
        Research.SoldierEffectivenessResearchCost += Research.SoldierEffectivenessResearchCostIncrement;
        Staff.SoldierEffectiveness += 10;
        AdvanceTime();
    }

    public bool CanResearchSoldierSurvivability()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.SoldierSurvivabilityResearchCost;

    public void ResearchSoldierSurvivability()
    {
        Debug.Assert(CanResearchSoldierSurvivability());
        Money.SubtractMoney(Research.SoldierSurvivabilityResearchCost);
        Research.SoldierSurvivabilityResearchCost += Research.SoldierSurvivabilityResearchCostIncrement;
        Staff.SoldierSurvivability += 10;
        AdvanceTime();
    }

    public void Reset()
    {
        Storage.Reset();
        Timeline.Reset();
        StateRefresh.Trigger();
    }
}
