﻿using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

// kja rename "Game" class to "Model" and move it to ViewModel, as a hook point for UI
// to access logic. Remove all nontrivial logic from it, pushing down to relevant classes.
public class Game
{
    public readonly Timeline Timeline;
    public readonly Accounting Accounting;
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
        Accounting accounting,
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
        Accounting = accounting;
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
        => !PlayerScore.GameOver && Accounting.CurrentMoney >= Research.MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        Accounting.PayForResearch(Research.MoneyRaisingMethodsResearchCost);
        Research.MoneyRaisingMethodsResearchCost += Research.MoneyRaisingMethodsResearchCostIncrement;
        Accounting.Data.MoneyRaisedPerActionAmount += 25;
        AdvanceTime();
    }

    public bool CanResearchTransportCapacity()
        => !PlayerScore.GameOver && Accounting.CurrentMoney >= Research.TransportCapacityResearchCost;

    public void ResearchTransportCapacity()
    {
        Debug.Assert(CanResearchTransportCapacity());
        Accounting.PayForResearch(Research.TransportCapacityResearchCost);
        Research.TransportCapacityResearchCost += Research.TransportCapacityResearchCostIncrement;
        MissionPrep.Data.ImproveTransportCapacity();
        AdvanceTime();
    }

    public bool CanResearchAgentEffectiveness()
        => !PlayerScore.GameOver && Accounting.CurrentMoney >= Research.AgentEffectivenessResearchCost;

    public void ResearchAgentEffectiveness()
    {
        Debug.Assert(CanResearchAgentEffectiveness());
        Accounting.PayForResearch(Research.AgentEffectivenessResearchCost);
        Research.AgentEffectivenessResearchCost += Research.AgentEffectivenessResearchCostIncrement;
        Staff.Data.AgentEffectiveness += 25;
        AdvanceTime();
    }

    public bool CanResearchAgentSurvivability()
        => !PlayerScore.GameOver && Accounting.CurrentMoney >= Research.AgentSurvivabilityResearchCost;

    public void ResearchAgentSurvivability()
    {
        Debug.Assert(CanResearchAgentSurvivability());
        Accounting.PayForResearch(Research.AgentSurvivabilityResearchCost);
        Research.AgentSurvivabilityResearchCost += Research.AgentSurvivabilityResearchCostIncrement;
        Staff.Data.AgentSurvivability += 25;
        AdvanceTime();
    }

    public bool CanResearchAgentRecoverySpeed()
        => !PlayerScore.GameOver && Accounting.CurrentMoney >= Research.AgentRecoverySpeedResearchCost;

    public void ResearchAgentRecoverySpeed()
    {
        Debug.Assert(CanResearchAgentRecoverySpeed());
        Accounting.PayForResearch(Research.AgentRecoverySpeedResearchCost);
        Research.AgentRecoverySpeedResearchCost += Research.AgentRecoverySpeedResearchCostIncrement;
        Staff.Data.ImproveAgentRecoverySpeed();
        AdvanceTime();
    }

    private void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!PlayerScore.GameOver);
        Timeline.AdvanceTime();
        PendingMission.AdvanceTime();
        Factions.AdvanceTime();
        Staff.AdvanceTime();
        Accounting.AdvanceTime(raisedMoney);
        _gameState.PersistGameState();
        StateRefresh.Trigger();
    }
}
