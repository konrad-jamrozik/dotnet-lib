using UfoGame.Model;

namespace UfoGame.ViewModel;

class HireAgentsPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
    private readonly StateRefresh _stateRefresh;

    public HireAgentsPlayerAction(Staff staff, StateRefresh stateRefresh)
    {
        _staff = staff;
        _stateRefresh = stateRefresh;
    }

    public void Act()
    {
        _staff.HireAgents();
        _stateRefresh.Trigger();
    }

    public string ActLabel()
        => $"Hire {_staff.Data.AgentsToHire} agents for ${_staff.AgentsToHireCost}";

    public int Input
    {
        get => _staff.Data.AgentsToHire;
        set => _staff.Data.AgentsToHire = value;
    }

    public void IncrementInput() => _staff.Data.AgentsToHire += 1;

    public void DecrementInput() => _staff.Data.AgentsToHire -= 1;

    public bool CanAct() => _staff.CanHireAgents();

    public bool CanAct(int value) => _staff.CanHireAgents(value);

    public int InputMax() => _staff.MaxAgentsToHire;

    public int InputMin() => _staff.MinAgentsToHire;
}