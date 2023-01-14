using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrep
{
    [JsonInclude]
    public readonly MissionPrepData Data;

    public int MaxSoldiersToSend => Math.Min(_staff.Data.ReadySoldiers, Data.TransportCapacity);

    public int MinSoldiersToSend => 1;

    public void NarrowSoldiersToSend()
        => Data.SoldiersToSend = Math.Max(MinSoldiersToSend, Math.Min(Data.SoldiersToSend, MaxSoldiersToSend));
    
    private readonly Staff _staff;

    public MissionPrep(MissionPrepData data, Staff staff)
    {
        Data = data;
        _staff = staff;
        NarrowSoldiersToSend();
    }
}