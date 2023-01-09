using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Staff
{
    private const int SoldierPrice = 30;

    [JsonInclude]
    public int CurrentSoldiers { get; private set; } = 0;

    [JsonInclude]
    public int SoldierEffectiveness = 100;

    [JsonInclude]
    public int SoldierSurvivability = 100;

    [JsonInclude]
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

    public bool CanHireSoldiers(int offset = 0)
    {
        if (_playerScore.GameOver)
            return false;

        if (!WithinRange(SoldiersToHire) && SoldiersToHire > MinSoldiersToHire)
            NarrowSoldiersToHire();

        return WithinRange(SoldiersToHire + offset);

        bool WithinRange(int soldiersToHire)
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
        Console.Out.WriteLine("Narrowing soldiers to hire! " + SoldiersToHire);
        SoldiersToHire = Math.Max(MinSoldiersToHire, Math.Min(SoldiersToHire, MaxSoldiersToHire));
        Console.Out.WriteLine("DONE Narrowing soldiers to hire! " + SoldiersToHire);
    }

    public void SubtractSoldiers(int amount)
    {
        CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Subtracted {amount} soldiers. Soldiers now at {CurrentSoldiers}.");
    }
}

