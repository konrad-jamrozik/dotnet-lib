using UfoGame.Model;

namespace UfoGame.ViewModel;

class HireSoldiersPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;

    public HireSoldiersPlayerAction(Staff staff)
        => _staff = staff;

    public void Act()
        => _staff.HireSoldiers();

    public string ActLabel()
        => $"Hire {_staff.SoldiersToHire} soldiers for ${_staff.SoldiersToHireCost}";

    public int Input
    {
        get => _staff.SoldiersToHire;
        set => _staff.SoldiersToHire = value;
    }

    public void IncrementInput()
        => _staff.IncrementSoldiersToHire();

    public void DecrementInput()
        => _staff.DecrementSoldiersToHire();

    public bool CanAct()
        => _staff.CanHireSoldiers();

    public bool CanAct(int value)
        => _staff.CanHireSoldiers(value);

    public int InputMax()
        => _staff.MaxSoldiersToHire;

    public int InputMin()
        => _staff.MinSoldiersToHire;
}