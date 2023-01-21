using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Game
{
    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;
    public readonly Archive Archive;
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
        Archive archive,
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
        Archive = archive;
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

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
        => AdvanceTime(raisedMoney: true);

    public bool CanResearchMoneyRaisingMethods()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Money.PayForResearch(Research.MoneyRaisingMethodsResearchCost);
        Research.MoneyRaisingMethodsResearchCost += Research.MoneyRaisingMethodsResearchCostIncrement;
        Money.Data.MoneyRaisedPerActionAmount += 25;
        AdvanceTime();
    }

    public bool CanResearchTransportCapacity()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.TransportCapacityResearchCost;

    public void ResearchTransportCapacity()
    {
        Debug.Assert(CanResearchTransportCapacity());
        Money.PayForResearch(Research.TransportCapacityResearchCost);
        Research.TransportCapacityResearchCost += Research.TransportCapacityResearchCostIncrement;
        MissionPrep.Data.ImproveTransportCapacity();
        AdvanceTime();
    }

    public bool CanResearchAgentEffectiveness()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.AgentEffectivenessResearchCost;

    public void ResearchAgentEffectiveness()
    {
        Debug.Assert(CanResearchAgentEffectiveness());
        Money.PayForResearch(Research.AgentEffectivenessResearchCost);
        Research.AgentEffectivenessResearchCost += Research.AgentEffectivenessResearchCostIncrement;
        Staff.Data.AgentEffectiveness += 25;
        AdvanceTime();
    }

    public bool CanResearchAgentSurvivability()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.AgentSurvivabilityResearchCost;

    public void ResearchAgentSurvivability()
    {
        Debug.Assert(CanResearchAgentSurvivability());
        Money.PayForResearch(Research.AgentSurvivabilityResearchCost);
        Research.AgentSurvivabilityResearchCost += Research.AgentSurvivabilityResearchCostIncrement;
        Staff.Data.AgentSurvivability += 25;
        AdvanceTime();
    }

    public bool CanResearchAgentRecoverySpeed()
        => !PlayerScore.GameOver && Money.CurrentMoney >= Research.AgentRecoverySpeedResearchCost;

    public void ResearchAgentRecoverySpeed()
    {
        Debug.Assert(CanResearchAgentRecoverySpeed());
        Money.PayForResearch(Research.AgentRecoverySpeedResearchCost);
        Research.AgentRecoverySpeedResearchCost += Research.AgentRecoverySpeedResearchCostIncrement;
        Staff.Data.ImproveAgentRecoverySpeed();
        AdvanceTime();
    }

    private void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!PlayerScore.GameOver);
        Timeline.IncrementTime();
        PendingMission.AdvanceMissionTime();
        Factions.AdvanceFactionsTime();
        Staff.AdvanceTime();
        Money.AdvanceTime(raisedMoney);
        _gameState.PersistGameState();
        StateRefresh.Trigger();
    }
}
