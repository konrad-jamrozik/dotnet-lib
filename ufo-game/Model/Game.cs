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
        Research research)
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
    }

    public bool CanDoNothing() => !PlayerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
        => AdvanceTime(raisedMoney: true);

    public bool CanResearchMoneyRaisingMethods()
        => Research.CanResearchMoneyRaisingMethods();

    public void ResearchMoneyRaisingMethods()
        => Research.ResearchMoneyRaisingMethods();

    public bool CanResearchTransportCapacity()
        => Research.CanResearchTransportCapacity();

    public void ResearchTransportCapacity()
        => Research.ResearchTransportCapacity();

    public bool CanResearchAgentEffectiveness()
        => Research.CanResearchAgentEffectiveness();

    public void ResearchAgentEffectiveness()
        => Research.ResearchAgentEffectiveness();

    public bool CanResearchAgentSurvivability()
        => Research.CanResearchAgentSurvivability();

    public void ResearchAgentSurvivability()
        => Research.ResearchAgentSurvivability();

    public bool CanResearchAgentRecoverySpeed()
        => Research.CanResearchAgentRecoverySpeed();

    public void ResearchAgentRecoverySpeed()
        => Research.ResearchAgentRecoverySpeed();

    private void AdvanceTime(bool raisedMoney = false)
        => Timeline.AdvanceTime(raisedMoney);
}
