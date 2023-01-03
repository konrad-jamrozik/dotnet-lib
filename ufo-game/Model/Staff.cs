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

    public void IncrementSoldiersToHire() => SoldiersToHire += 1;

    public void DecrementSoldiersToHire() => SoldiersToHire -= 1;

    public int SoldiersToHireCost => SoldiersToHire * SoldierPrice;

    public int MinSoldiersToHire => 1;

    public int MaxSoldiersToHire => _money.CurrentMoney / SoldierPrice;

    private readonly Money _money;
    private readonly OperationsArchive _archive;
    private readonly PlayerScore _playerScore;


    public Staff(Money money, PlayerScore playerScore, OperationsArchive archive)
    {
        _money = money;
        _playerScore = playerScore;
        _archive = archive;
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
        NarrowSoldiersToHire();
        Console.Out.WriteLine($"Hired {SoldiersToHire} soldiers. Soldiers now at {CurrentSoldiers}.");
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

