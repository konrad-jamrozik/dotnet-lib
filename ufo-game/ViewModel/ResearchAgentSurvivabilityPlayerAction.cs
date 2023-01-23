using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchAgentSurvivabilityPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;

    public ResearchAgentSurvivabilityPlayerAction(Research research)
    {
        _research = research;
    }

    public bool CanAct() => _research.CanResearchAgentSurvivability();

    public void Act() => _research.ResearchAgentSurvivability();

    public string ActLabel() => $"Research agent survivability for {_research.Data.AgentSurvivabilityResearchCost}";
}