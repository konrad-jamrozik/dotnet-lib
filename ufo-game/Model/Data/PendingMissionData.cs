using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class PendingMissionData
{
    public static PendingMissionData New(PlayerScore playerScore, Random random, Factions factions)
        => !playerScore.GameOver
            ? NewValid(playerScore, random, factions)
            : NewEmpty;

    public static PendingMissionData NewEmpty => new PendingMissionData();

    private static PendingMissionData NewValid(
        PlayerScore playerScore,
        Random random,
        Factions factions)
    {
        Debug.Assert(!playerScore.GameOver);
        return new PendingMissionData(
            availableIn: random.Next(1, 6 + 1),
            expiresIn: 3,
            moneyRewardCoefficient: random.Next(5, 15 + 1) / (float)10,
            enemyPowerCoefficient: random.Next(5, 15 + 1) / (float)10,
            factionName: factions.RandomUndefeatedFaction.Name);
    }

    [JsonInclude] public int AvailableIn;
    [JsonInclude] public int ExpiresIn;
    [JsonInclude] public float MoneyRewardCoefficient;
    [JsonInclude] public float EnemyPowerCoefficient;
    [JsonInclude] public string FactionName = Factions.NoFaction;

    public bool IsNoMission => FactionName == Factions.NoFaction;

    public PendingMissionData(
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

    public PendingMissionData()
        => Reset();

    private void Reset()
    {
        AvailableIn = 0;
        ExpiresIn = 0;
        MoneyRewardCoefficient = 0;
        EnemyPowerCoefficient = 0;
        FactionName = Factions.NoFaction;
    }
}