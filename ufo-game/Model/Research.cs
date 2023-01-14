using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Research
{
    [JsonInclude] public int MoneyRaisingMethodsResearchCost;
    [JsonInclude] public int TransportCapacityResearchCost;
    [JsonInclude] public int SoldierEffectivenessResearchCost;
    [JsonInclude] public int SoldierSurvivabilityResearchCost;
    [JsonInclude] public int SoldierRecoverySpeedResearchCost;
    

    public const int MoneyRaisingMethodsResearchCostIncrement = 10;
    public const int TransportCapacityResearchCostIncrement = 200;
    public const int SoldierEffectivenessResearchCostIncrement = 10;
    public const int SoldierSurvivabilityResearchCostIncrement = 10;
    public const int SoldierRecoverySpeedResearchCostIncrement = 100;

    public Research()
        => Reset();

    public void Reset()
    {
        MoneyRaisingMethodsResearchCost = 100;
        TransportCapacityResearchCost = 1000;
        SoldierEffectivenessResearchCost = 100;
        SoldierSurvivabilityResearchCost = 100;
        SoldierRecoverySpeedResearchCost = 500;
    }
}