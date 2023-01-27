using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionSiteData
{
    public static MissionSiteData New(PlayerScore playerScore, Random random, FactionsData factionsData)
        => !playerScore.GameOver
            ? NewValid(playerScore, random, factionsData)
            : NewEmpty;

    public static MissionSiteData NewEmpty => new MissionSiteData();

    private static MissionSiteData NewValid(
        PlayerScore playerScore,
        Random random,
        FactionsData factionsData)
    {
        Debug.Assert(!playerScore.GameOver);
        return new MissionSiteData(
            availableIn: random.Next(1, 6 + 1),
            expiresIn: 3,
            moneyRewardCoefficient: random.Next(5, 15 + 1) / (float)10,
            enemyPowerCoefficient: random.Next(5, 15 + 1) / (float)10,
            factionName: factionsData.RandomUndefeatedFactionData.Name);
    }

    [JsonInclude] public int AvailableIn;
    [JsonInclude] public int ExpiresIn;
    [JsonInclude] public float MoneyRewardCoefficient;
    [JsonInclude] public float EnemyPowerCoefficient;
    [JsonInclude] public string FactionName;

    public bool IsNoMission => FactionName == FactionsData.NoFaction;

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
}