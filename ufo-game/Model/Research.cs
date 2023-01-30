using System.Diagnostics;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Research
{
    public readonly ResearchData Data;
    private readonly Timeline _timeline;
    private readonly Accounting _accounting;
    private readonly MissionDeployment _missionDeployment;
    private readonly StaffData _staffData;
    private readonly PlayerScore _playerScore;
    private readonly SickBay _sickBay;

    public Research(
        ResearchData data,
        Timeline timeline,
        Accounting accounting,
        MissionDeployment missionDeployment,
        StaffData staffData,
        PlayerScore playerScore,
        SickBay sickBay)
    {
        Data = data;
        _timeline = timeline;
        _accounting = accounting;
        _missionDeployment = missionDeployment;
        _staffData = staffData;
        _playerScore = playerScore;
        _sickBay = sickBay;
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
        _missionDeployment.Data.ImproveTransportCapacity();
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentEffectiveness()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentEffectivenessResearchCost;

    public void ResearchAgentEffectiveness()
    {
        Debug.Assert(CanResearchAgentEffectiveness());
        _accounting.PayForResearch(Data.AgentEffectivenessResearchCost);
        Data.AgentEffectivenessResearchCost += ResearchData.AgentEffectivenessResearchCostIncrement;
        _staffData.AgentEffectiveness += 25;
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentSurvivability()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentSurvivabilityResearchCost;

    public void ResearchAgentSurvivability()
    {
        Debug.Assert(CanResearchAgentSurvivability());
        _accounting.PayForResearch(Data.AgentSurvivabilityResearchCost);
        Data.AgentSurvivabilityResearchCost += ResearchData.AgentSurvivabilityResearchCostIncrement;
        _staffData.AgentSurvivability += 25;
        _timeline.AdvanceTime();
    }

    public bool CanResearchAgentRecoverySpeed()
        => !_playerScore.GameOver && _accounting.CurrentMoney >= Data.AgentRecoverySpeedResearchCost;

    public void ResearchAgentRecoverySpeed()
    {
        Debug.Assert(CanResearchAgentRecoverySpeed());
        _accounting.PayForResearch(Data.AgentRecoverySpeedResearchCost);
        Data.AgentRecoverySpeedResearchCost += ResearchData.AgentRecoverySpeedResearchCostIncrement;
        _sickBay.Data.ImproveAgentRecoverySpeed();
        _timeline.AdvanceTime();
    }
}