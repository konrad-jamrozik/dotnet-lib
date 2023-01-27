using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionSitesData : IPersistable
{
    [JsonInclude] public List<MissionSiteData> Data { get; private set; } = new List<MissionSiteData>();

    public MissionSitesData()
        => Reset();

    public void Reset()
        => Data = new List<MissionSiteData> { MissionSiteData.NewEmpty };

    public void New(PlayerScore playerScore, Random random, FactionsData factionsData)
    {
        Data[0] = MissionSiteData.New(playerScore, random, factionsData);
    }
}