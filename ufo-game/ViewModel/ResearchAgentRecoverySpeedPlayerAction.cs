using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchAgentRecoverySpeedPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;

    public ResearchAgentRecoverySpeedPlayerAction(Research research)
    {
        _research = research;
    }

    public bool CanAct() => _research.CanResearchAgentRecoverySpeed();

    public void Act() => _research.ResearchAgentRecoverySpeed();

    public string ActLabel() => $"Research agent recovery speed for {_research.Data.AgentRecoverySpeedResearchCost}";
}