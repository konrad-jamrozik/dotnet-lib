using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Staff
{
    [JsonInclude]
    public readonly StaffData Data;

    private const int SoldierPrice = 30;

    public int SoldiersToHireCost => Data.SoldiersToHire * SoldierPrice;

    public int MinSoldiersToHire => 1;

    public int MaxSoldiersToHire => _money.CurrentMoney / SoldierPrice;

    private readonly Money _money;
    private readonly OperationsArchive _archive;
    private readonly PlayerScore _playerScore;

    public Staff(
        StaffData data,
        Money money,
        PlayerScore playerScore,
        OperationsArchive archive)
    {
        Data = data;
        _money = money;
        _playerScore = playerScore;
        _archive = archive;
    }

    public bool CanHireSoldiers(int offset = 0)
    {
        if (_playerScore.GameOver)
            return false;

        if (!WithinRange(Data.SoldiersToHire) && Data.SoldiersToHire > MinSoldiersToHire)
            NarrowSoldiersToHire();

        return WithinRange(Data.SoldiersToHire + offset);

        bool WithinRange(int soldiersToHire)
        {
            return MinSoldiersToHire <= soldiersToHire && soldiersToHire <= MaxSoldiersToHire;
        }
    }

    public void HireSoldiers()
    {
        Debug.Assert(CanHireSoldiers());
        _money.SubtractMoney(SoldiersToHireCost);
        _archive.RecordHiredSoldiers(Data.SoldiersToHire);
        Data.CurrentSoldiers += Data.SoldiersToHire;
        Console.Out.WriteLine($"Hired {Data.SoldiersToHire} soldiers. Soldiers now at {Data.CurrentSoldiers}.");
    }

    private void NarrowSoldiersToHire()
    {
        Data.SoldiersToHire = Math.Max(MinSoldiersToHire, Math.Min(Data.SoldiersToHire, MaxSoldiersToHire));
    }

    public void SubtractSoldiers(int amount)
    {
        Data.CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Subtracted {amount} soldiers. Soldiers now at {Data.CurrentSoldiers}.");
    }

    public void AdvanceTime()
    {
        Data.AdvanceTime();
    }
}

