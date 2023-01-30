using System.Text.Json.Serialization;
using UfoGame.Infra;

namespace UfoGame.Model.Data;

public class MissionSitesData : IPersistable
{
    [JsonInclude] public List<MissionSiteData> Data { get; private set; } = new List<MissionSiteData>();

    public MissionSitesData()
        => Reset();

    public void New(PlayerScore playerScore, RandomGen randomGen, FactionsData factionsData)
    {
        Data[0] = MissionSiteData.New(playerScore, randomGen, factionsData);
    }

    public void Reset()
        => Data = new List<MissionSiteData> { MissionSiteData.NewEmpty };
}