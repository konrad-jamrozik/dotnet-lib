using UfoGame.Model;

namespace UfoGame.ViewModel;

class HireAgentsPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Procurement _procurement;
    private readonly ViewStateRefresh _viewStateRefresh;

    public HireAgentsPlayerAction(Procurement procurement, ViewStateRefresh viewStateRefresh)
    {
        _procurement = procurement;
        _viewStateRefresh = viewStateRefresh;
    }

    public void Act()
    {
        _procurement.HireAgents();
        _viewStateRefresh.Trigger();
    }

    public string ActLabel()
        => $"Hire {_procurement.Data.AgentsToHire} agents for ${_procurement.AgentsToHireCost}";

    public int Input
    {
        get => _procurement.Data.AgentsToHire;
        set => _procurement.Data.AgentsToHire = value;
    }

    public void IncrementInput() => _procurement.Data.AgentsToHire += 1;

    public void DecrementInput() => _procurement.Data.AgentsToHire -= 1;

    public bool CanAct() => _procurement.CanHireAgents();

    public bool CanAct(int value) => _procurement.CanHireAgents(value);

    public int InputMax() => _procurement.MaxAgentsToHire;

    public int InputMin() => _procurement.MinAgentsToHire;
}