using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrep
{
    private readonly MissionPrepData _data;

    [JsonIgnore]
    public int SoldiersToSend
    {
        get => _data.SoldiersToSend;
        set => _data.SoldiersToSend = value;
    }

    public int MaxSoldiersToSend => _staff.CurrentSoldiers;

    public int MinSoldiersToSend => 1;

    public void NarrowSoldiersToSend()
        => _data.SoldiersToSend = Math.Max(MinSoldiersToSend, Math.Min(_data.SoldiersToSend, MaxSoldiersToSend));
    
    private readonly Staff _staff;

    public MissionPrep(MissionPrepData data, Staff staff)
    {
        _staff = staff;
        _data = data;
        NarrowSoldiersToSend();
    }
}