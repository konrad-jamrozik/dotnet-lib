namespace UfoGame.Model;

public class MissionPrep
{
    public int SoldiersToSend { get; set; }

    public void IncrementSoldiersToSend() => SoldiersToSend += 1;

    public void DecrementSoldiersToSend() => SoldiersToSend -= 1;

    public int MaxSoldiersToSend => _staff.CurrentSoldiers;

    public int MinSoldiersToSend => 1;

    public void NarrowSoldiersToSend()
        => SoldiersToSend = Math.Max(MinSoldiersToSend, Math.Min(SoldiersToSend, MaxSoldiersToSend));
    
    private readonly Staff _staff;

    public MissionPrep(Staff staff)
    {
        _staff = staff;
        SoldiersToSend = MinSoldiersToSend;
    }
}