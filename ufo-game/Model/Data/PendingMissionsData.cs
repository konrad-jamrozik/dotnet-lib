using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class PendingMissionsData : IDeserializable
{
    [JsonInclude] public List<PendingMissionData> Data { get; private set; } = new List<PendingMissionData>();

    public PendingMissionsData()
        => Reset();

    public void Reset()
        => Data = new List<PendingMissionData> { PendingMissionData.NewEmpty };

    public void New(PlayerScore playerScore, Random random, FactionsData factionsData)
    {
        Data[0] = PendingMissionData.New(playerScore, random, factionsData);
    }
}