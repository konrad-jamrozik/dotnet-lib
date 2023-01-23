using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchAgentEffectivenessPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;

    public ResearchAgentEffectivenessPlayerAction(Research research)
    {
        _research = research;
    }

    public bool CanAct() => _research.CanResearchAgentEffectiveness();

    public void Act() => _research.ResearchAgentEffectiveness();

    public string ActLabel() => $"Research agent effectiveness for {_research.Data.AgentEffectivenessResearchCost}";
}