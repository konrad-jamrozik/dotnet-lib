using System.Text.Json.Serialization;

namespace UfoGame.Model
{
    public class PendingMissionData
    {
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

        public PendingMissionData(
            int availableIn,
            int expiresIn,
            int moneyReward,
            float enemyPowerCoefficient,
            string factionName)
        {
            AvailableIn = availableIn;
            ExpiresIn = expiresIn;
            MoneyReward = moneyReward;
            EnemyPowerCoefficient = enemyPowerCoefficient;
            FactionName = factionName;
        }
    }
}
