using System.Diagnostics;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Research
{
    public readonly ResearchData Data;
    private readonly Accounting _accounting;
    private readonly MissionPrep _missionPrep;
    private readonly Staff _staff;
    private readonly Timeline _timeline;
    private readonly PlayerScore _playerScore;

    public Research(
        ResearchData data,
        Timeline timeline,
        Accounting accounting,
        MissionPrep missionPrep,
        Staff staff,
        PlayerScore playerScore)
    {
        Data = data;
        _timeline = timeline;
        _accounting = accounting;
        _missionPrep = missionPrep;
        _staff = staff;
        _playerScore = playerScore;
    }

    public bool CanResearchMoneyRaisingMethods()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.MoneyRaisingMethodsResearchCost;

    public void ResearchMoneyRaisingMethods()
    {
        Debug.Assert(CanResearchMoneyRaisingMethods());
        _accounting.PayForResearch(Data.MoneyRaisingMethodsResearchCost);
        Data.MoneyRaisingMethodsResearchCost += ResearchData.MoneyRaisingMethodsResearchCostIncrement;
        _accounting.Data.MoneyRaisedPerActionAmount += 25;
        _timeline.AdvanceTime();
    }

    public bool CanResearchTransportCapacity()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.TransportCapacityResearchCost;

    public void ResearchTransportCapacity()
    {
        Debug.Assert(CanResearchTransportCapacity());
        _accounting.PayForResearch(Data.TransportCapacityResearchCost);
        Data.TransportCapacityResearchCost += ResearchData.TransportCapacityResearchCostIncrement;
        _missionPrep.Data.ImproveTransportCapacity();
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentEffectiveness()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentEffectivenessResearchCost;

    public void ResearchAgentEffectiveness()
    {
        Debug.Assert(CanResearchAgentEffectiveness());
        _accounting.PayForResearch(Data.AgentEffectivenessResearchCost);
        Data.AgentEffectivenessResearchCost += ResearchData.AgentEffectivenessResearchCostIncrement;
        _staff.Data.AgentEffectiveness += 25;
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentSurvivability()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentSurvivabilityResearchCost;

    public void ResearchAgentSurvivability()
    {
        Debug.Assert(CanResearchAgentSurvivability());
        _accounting.PayForResearch(Data.AgentSurvivabilityResearchCost);
        Data.AgentSurvivabilityResearchCost += ResearchData.AgentSurvivabilityResearchCostIncrement;
        _staff.Data.AgentSurvivability += 25;
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentRecoverySpeed()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentRecoverySpeedResearchCost;

    public void ResearchAgentRecoverySpeed()
    {
        Debug.Assert(CanResearchAgentRecoverySpeed());
        _accounting.PayForResearch(Data.AgentRecoverySpeedResearchCost);
        Data.AgentRecoverySpeedResearchCost += ResearchData.AgentRecoverySpeedResearchCostIncrement;
        _staff.Data.ImproveAgentRecoverySpeed();
        _timeline.AdvanceTime();
    }
}