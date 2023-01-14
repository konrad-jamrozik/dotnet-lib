using UfoGame.Model;

namespace UfoGame.ViewModel;

class FireSoldiersPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
    private readonly StateRefresh _stateRefresh;

    public FireSoldiersPlayerAction(Staff staff, StateRefresh stateRefresh)
    {
        _staff = staff;
        _stateRefresh = stateRefresh;
    }

    public void Act()
    {
        _staff.FireSoldiers();
        _stateRefresh.Trigger();
    }

    public string ActLabel()
        => $"Fire {_staff.Data.SoldiersToFire} soldiers";

    public int Input
    {
        get => _staff.Data.SoldiersToFire;
        set => _staff.Data.SoldiersToFire = value;
    }

    public void IncrementInput() => _staff.Data.SoldiersToFire += 1;

    public void DecrementInput() => _staff.Data.SoldiersToFire -= 1;

    public bool CanAct() => _staff.CanFireSoldiers();

    public bool CanAct(int value) => _staff.CanFireSoldiers(value);

    public int InputMax() => _staff.MaxSoldiersToFire;

    public int InputMin() => _staff.MinSoldiersToFire;
}