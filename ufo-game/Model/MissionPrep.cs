using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrep
{
    [JsonInclude]
    public readonly MissionPrepData Data;

    public int MinAgentsSendableOnMission => 1;

    public int MaxAgentsSendableOnMission => Math.Min(
        Data.TransportCapacity,
        _staff.Data.AgentsSendableOnMissionCount);

    private readonly Staff _staff;

    public MissionPrep(MissionPrepData data, Staff staff)
    {
        Data = data;
        _staff = staff;
    }
}