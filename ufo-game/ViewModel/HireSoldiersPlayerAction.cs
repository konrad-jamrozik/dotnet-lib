using UfoGame.Model;

namespace UfoGame.ViewModel;

class HireSoldiersPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
    private readonly StateRefresh _stateRefresh;

    public HireSoldiersPlayerAction(Staff staff, StateRefresh stateRefresh)
    {
        _staff = staff;
        _stateRefresh = stateRefresh;
    }

    public void Act()
    {
        _staff.HireSoldiers();
        _stateRefresh.Trigger();
    }

    public string ActLabel()
        => $"Hire {_staff.Data.SoldiersToHire} soldiers for ${_staff.SoldiersToHireCost}";

    public int Input
    {
        get => _staff.Data.SoldiersToHire;
        set => _staff.Data.SoldiersToHire = value;
    }

    public void IncrementInput() => _staff.Data.SoldiersToHire += 1;

    public void DecrementInput() => _staff.Data.SoldiersToHire -= 1;

    public bool CanAct() => _staff.CanHireSoldiers();

    public bool CanAct(int value) => _staff.CanHireSoldiers(value);

    public int InputMax() => _staff.MaxSoldiersToHire;

    public int InputMin() => _staff.MinSoldiersToHire;
}