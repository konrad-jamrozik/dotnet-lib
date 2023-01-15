using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Staff
{
    [JsonInclude]
    public readonly StaffData Data;

    private const int SoldierPrice = 50;

    // kja this probably should be on Money; not this is used in UI
    public int SoldiersToHireCost => Data.SoldiersToHire * SoldierPrice;

    public int MinSoldiersToHire => 1;

    public int MaxSoldiersToHire => _money.CurrentMoney / SoldierPrice;

    public int MinSoldiersToFire => 1;

    public int MaxSoldiersToFire => Data.ReadySoldiers;

    private readonly Money _money;
    private readonly Archive _archive;
    private readonly PlayerScore _playerScore;
    private readonly Timeline _timeline;

    public Staff(
        StaffData data,
        Money money,
        PlayerScore playerScore,
        Archive archive,
        Timeline timeline)
    {
        Data = data;
        _money = money;
        _playerScore = playerScore;
        _archive = archive;
        _timeline = timeline;
    }

    public bool CanHireSoldiers(int offset = 0)
    {
        if (_playerScore.GameOver)
            return false;

        if (!WithinRange(Data.SoldiersToHire) && Data.SoldiersToHire > MinSoldiersToHire)
            NarrowSoldiersToHire();

        return WithinRange(Data.SoldiersToHire + offset);

        bool WithinRange(int soldiersToHire)
            => MinSoldiersToHire <= soldiersToHire && soldiersToHire <= MaxSoldiersToHire;

        void NarrowSoldiersToHire()
            => Data.SoldiersToHire = Math.Max(MinSoldiersToHire, Math.Min(Data.SoldiersToHire, MaxSoldiersToHire));
    }

    public void HireSoldiers()
    {
        // kja this may FIRST narrow soldiers to hire and THEN pass. This is unexpected from user POV.
        // Same with SoldiersToSend and SoldiersToFire.
        // Need to pass something like: allowNarrowing: false.
        Debug.Assert(CanHireSoldiers());
        _money.PayForHiringSoldiers(SoldiersToHireCost);
        _archive.RecordHiredSoldiers(Data.SoldiersToHire);
        Data.CurrentSoldiers += Data.SoldiersToHire;
        Data.HireSoldiers(_timeline.CurrentTime);
        Console.Out.WriteLine($"Hired {Data.SoldiersToHire} soldiers. Soldiers now at {Data.CurrentSoldiers}.");
    }

    public bool CanFireSoldiers(int offset = 0)
    {
        if (_playerScore.GameOver)
            return false;

        if (!WithinRange(Data.SoldiersToFire) && Data.SoldiersToFire > MinSoldiersToHire)
            NarrowSoldiersToFire();

        return WithinRange(Data.SoldiersToFire + offset);

        bool WithinRange(int soldiersToFire)
            => MinSoldiersToFire <= soldiersToFire && soldiersToFire <= MaxSoldiersToFire;

        void NarrowSoldiersToFire()
            => Data.SoldiersToFire = Math.Max(MinSoldiersToFire, Math.Min(Data.SoldiersToFire, MaxSoldiersToFire));
    }

    public void FireSoldiers()
    {
        Debug.Assert(CanFireSoldiers());
        Debug.Assert(Data.ReadySoldiers >= Data.SoldiersToFire);
        _archive.RecordFiredSoldiers(Data.SoldiersToFire);
        Data.CurrentSoldiers -= Data.SoldiersToFire;
        Console.Out.WriteLine($"Fired {Data.SoldiersToFire} soldiers. Soldiers now at {Data.CurrentSoldiers}.");
    }

    public void LoseSoldiers(int amount)
    {
        Data.CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Lost {amount} soldiers. Soldiers now at {Data.CurrentSoldiers}.");
    }

    public void AdvanceTime()
    {
        Data.AdvanceTime();
    }
}

