using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrep
{
    [JsonInclude]
    public readonly MissionPrepData Data;

    public int MinSoldiersSendableOnMission => 1;

    public int MaxSoldiersSendableOnMission => Math.Min(
        Data.TransportCapacity,
        _staff.Data.SoldiersSendableOnMissionCount);

    private readonly Staff _staff;

    public MissionPrep(MissionPrepData data, Staff staff)
    {
        Data = data;
        _staff = staff;
    }
}