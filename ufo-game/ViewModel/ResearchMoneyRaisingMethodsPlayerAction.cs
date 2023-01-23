using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchMoneyRaisingMethodsPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;

    public ResearchMoneyRaisingMethodsPlayerAction(Research research)
    {
        _research = research;
    }

    public bool CanAct() => _research.CanResearchMoneyRaisingMethods();

    public void Act() => _research.ResearchMoneyRaisingMethods();

    public string ActLabel() => $"Research money raising methods for {_research.Data.MoneyRaisingMethodsResearchCost}";
}