using System.Diagnostics;

namespace UfoGame.Model;

public class Game
{
    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly OperationsArchive OperationsArchive;
    public readonly MissionPrep MissionPrep;
    public readonly PendingMission PendingMission;
    public readonly Factions Factions;
    public readonly PlayerScore PlayerScore;
    public readonly Research Research;
    public readonly StateRefresh StateRefresh;
    private readonly GameState _gameState;

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
        Research research,
        GameState gameState)
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
        Research = research;
        _gameState = gameState;
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
        _gameState.PersistGameState();
        StateRefresh.Trigger();
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

    public bool CanResearchTransportCapacity()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.TransportCapacityResearchCost;

    public void ResearchTransportCapacity()
    {
        Debug.Assert(CanResearchTransportCapacity());
        Money.SubtractMoney(Research.TransportCapacityResearchCost);
        Research.TransportCapacityResearchCost += Research.TransportCapacityResearchCostIncrement;
        MissionPrep.Data.ImproveTransportCapacity();
        AdvanceTime();
    }

    public bool CanResearchSoldierEffectiveness()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.SoldierEffectivenessResearchCost;

    public void ResearchSoldierEffectiveness()
    {
        Debug.Assert(CanResearchSoldierEffectiveness());
        Money.SubtractMoney(Research.SoldierEffectivenessResearchCost);
        Research.SoldierEffectivenessResearchCost += Research.SoldierEffectivenessResearchCostIncrement;
        Staff.Data.SoldierEffectiveness += 10;
        AdvanceTime();
    }

    public bool CanResearchSoldierSurvivability()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.SoldierSurvivabilityResearchCost;

    public void ResearchSoldierSurvivability()
    {
        Debug.Assert(CanResearchSoldierSurvivability());
        Money.SubtractMoney(Research.SoldierSurvivabilityResearchCost);
        Research.SoldierSurvivabilityResearchCost += Research.SoldierSurvivabilityResearchCostIncrement;
        Staff.Data.SoldierSurvivability += 10;
        AdvanceTime();
    }
}
