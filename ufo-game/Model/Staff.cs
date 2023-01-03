using System.Diagnostics;

namespace UfoGame.Model;

public class Staff
{
    // kja sort out story with setters/getters.
    public int CurrentSoldiers { get; private set; } = 0;

    public int SoldierPrice { get; private set; } = 30;

    public int SoldierEffectiveness = 100;

    public int SoldierSurvivability = 100;

    public int SoldiersToHire = 1;

    public int SoldiersToHireCost => SoldiersToHire * SoldierPrice;

    public int MinSoldiersToHire => 1;

    public int MaxSoldiersToHire => _money.CurrentMoney / SoldierPrice;

    private readonly Money _money;
    private readonly OperationsArchive _archive;
    private readonly PlayerScore _playerScore;
    private readonly StateRefresh _stateRefresh;

    public Staff(
        Money money,
        PlayerScore playerScore,
        OperationsArchive archive,
        StateRefresh stateRefresh)
    {
        _money = money;
        _playerScore = playerScore;
        _archive = archive;
        _stateRefresh = stateRefresh;
    }

    public bool CanHireSoldiers(int? soldiersToHire = null)
    {
        soldiersToHire ??= SoldiersToHire;
        if (_playerScore.GameOver)
            return false;
        if (!WithinRange())
            // We need to narrow here, otherwise we are risking forgetting to narrow
            // the value, thus disabling the input control until this "Can" method returns true again.
            NarrowSoldiersToHire();
        return WithinRange();

        bool WithinRange()
        {
            return MinSoldiersToHire <= soldiersToHire && soldiersToHire <= MaxSoldiersToHire;
        }
    }

    public void HireSoldiers()
    {
        Debug.Assert(CanHireSoldiers());
        _money.SubtractMoney(SoldiersToHireCost);
        _archive.RecordHiredSoldiers(SoldiersToHire);
        CurrentSoldiers += SoldiersToHire;
        Console.Out.WriteLine($"Hired {SoldiersToHire} soldiers. Soldiers now at {CurrentSoldiers}.");
        _stateRefresh.Trigger();
    }

    private void NarrowSoldiersToHire()
    {
        Console.Out.WriteLine("Narrowing soldiers to hire!");
        SoldiersToHire = Math.Max(MinSoldiersToHire, Math.Min(SoldiersToHire, MaxSoldiersToHire));
    }

    public void SubtractSoldiers(int amount)
    {
        CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Subtracted {amount} soldiers. Soldiers now at {CurrentSoldiers}.");
    }
}

