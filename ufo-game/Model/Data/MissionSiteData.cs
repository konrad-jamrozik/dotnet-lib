using System.Diagnostics;
using System.Text.Json.Serialization;
using UfoGame.Infra;

namespace UfoGame.Model.Data;

public class MissionSiteData
{
    [JsonInclude] public int AvailableIn;
    [JsonInclude] public int ExpiresIn;
    [JsonInclude] public float MoneyRewardCoefficient { get; private set; }
    [JsonInclude] public float EnemyPowerCoefficient { get; private set; }
    [JsonInclude] public string FactionName { get; private set; }

    public MissionSiteData(
        int availableIn,
        int expiresIn,
        float moneyRewardCoefficient,
        float enemyPowerCoefficient,
        string factionName)
    {
        AvailableIn = availableIn;
        ExpiresIn = expiresIn;
        MoneyRewardCoefficient = moneyRewardCoefficient;
        EnemyPowerCoefficient = enemyPowerCoefficient;
        FactionName = factionName;
    }

    public MissionSiteData()
    {
        AvailableIn = 0;
        ExpiresIn = 0;
        MoneyRewardCoefficient = 0;
        EnemyPowerCoefficient = 0;
        FactionName = FactionsData.NoFaction;
    }

    public static MissionSiteData New(PlayerScore playerScore, RandomGen randomGen, FactionsData factionsData)
        => !playerScore.GameOver
            ? NewValid(playerScore, randomGen, factionsData)
            : NewEmpty;

    public static MissionSiteData NewEmpty => new MissionSiteData();

    public bool IsNoMission => FactionName == FactionsData.NoFaction;

    private static MissionSiteData NewValid(
        PlayerScore playerScore,
        RandomGen randomGen,
        FactionsData factionsData)
    {
        Debug.Assert(!playerScore.GameOver);
        return new MissionSiteData(
            availableIn: randomGen.Random.Next(1, 6 + 1),
            expiresIn: 3,
            moneyRewardCoefficient: randomGen.Random.Next(5, 15 + 1) / (float)10,
            enemyPowerCoefficient: randomGen.Random.Next(5, 15 + 1) / (float)10,
            factionName: factionsData.RandomUndefeatedFactionData(randomGen).Name);
    }
}