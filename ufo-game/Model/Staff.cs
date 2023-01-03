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
        return !_playerScore.GameOver 
               && soldiersToHire <= MaxSoldiersToHire
               && soldiersToHire >= MinSoldiersToHire;
    }

    public void HireSoldiers()
    {
        Debug.Assert(CanHireSoldiers());
        _money.SubtractMoney(SoldiersToHireCost);
        _archive.RecordHiredSoldiers(SoldiersToHire);
        CurrentSoldiers += SoldiersToHire;
        // kja bug: discovered bug with blocked input controls if money was spent in another way
        // Consider following scenario / repro:
        // 1. slider set to hire soldiers for 300$,
        // 2. player does research, money goes below 300$,
        // 3. but Narrow method is not executed on research, so the input controls get disabled
        // and player cannot change them until they get at least 300$ again.
        // Proposed solutions:
        // - call money-dependent narrow every time money is reduced
        // - OR if "CarHire" or equivalent returns false, try to narrow. This can be done in the getter.
        NarrowSoldiersToHire();
        Console.Out.WriteLine($"Hired {SoldiersToHire} soldiers. Soldiers now at {CurrentSoldiers}.");
        _stateRefresh.Trigger();
    }

    private void NarrowSoldiersToHire()
    {
        SoldiersToHire = Math.Max(MinSoldiersToHire, Math.Min(SoldiersToHire, MaxSoldiersToHire));
    }

    public void SubtractSoldiers(int amount)
    {
        CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Subtracted {amount} soldiers. Soldiers now at {CurrentSoldiers}.");
    }
}

