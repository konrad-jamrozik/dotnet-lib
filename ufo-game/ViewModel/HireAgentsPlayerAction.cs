using UfoGame.Model;

namespace UfoGame.ViewModel;

class HireAgentsPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Procurement _procurement;
    private readonly StateRefresh _stateRefresh;

    public HireAgentsPlayerAction(Procurement procurement, StateRefresh stateRefresh)
    {
        _procurement = procurement;
        _stateRefresh = stateRefresh;
    }

    public void Act()
    {
        _procurement.HireAgents();
        _stateRefresh.Trigger();
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