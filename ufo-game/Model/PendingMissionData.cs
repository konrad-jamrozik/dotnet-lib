using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMissionData
{
    public static PendingMissionData New(PlayerScore playerScore, Random random, Factions factions)
        => !playerScore.GameOver
            ? NewValid(playerScore, random, factions)
            : NewEmpty;

    private static PendingMissionData NewEmpty => new PendingMissionData();

    private static PendingMissionData NewValid(PlayerScore playerScore, Random random, Factions factions)
    {
        Debug.Assert(!playerScore.GameOver);
        return new PendingMissionData(
            availableIn: random.Next(1, 6 + 1),
            expiresIn: random.Next(1, 6 + 1),
            moneyReward: random.Next(10, 200 + 1),
            enemyPowerCoefficient: random.Next(5, 15 + 1) / (float)10,
            factionName: factions.RandomUndefeatedFaction.Name);
    }

    [JsonInclude]
    public int AvailableIn { get; set; }

    [JsonInclude]
    public int ExpiresIn { get; set; }

    [JsonInclude]
    public int MoneyReward { get; set; }
    
    [JsonInclude]
    public float EnemyPowerCoefficient { get; set; } 

    [JsonInclude]
    public string FactionName { get; set; }

    public bool IsNoMission => FactionName == Factions.NoFaction;

    public PendingMissionData(
        int availableIn = 0,
        int expiresIn = 0,
        int moneyReward = 0,
        float enemyPowerCoefficient = 1,
        string factionName = Factions.NoFaction)
    {
        AvailableIn = availableIn;
        ExpiresIn = expiresIn;
        MoneyReward = moneyReward;
        EnemyPowerCoefficient = enemyPowerCoefficient;
        FactionName = factionName;
    }
}