using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class PendingMissions
{
    [JsonInclude] public List<PendingMissionData> Data { get; private set; } = new List<PendingMissionData>();

    public PendingMissions()
        => Reset();

    public void Reset()
        => Data = new List<PendingMissionData> { PendingMissionData.NewEmpty };

    public void New(PlayerScore playerScore, Random random, Factions factions)
    {
        Data[0] = PendingMissionData.New(playerScore, random, factions);
    }
}