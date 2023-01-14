using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Research
{
    [JsonInclude] public int MoneyRaisingMethodsResearchCost;
    [JsonInclude] public int TransportCapacityResearchCost;
    [JsonInclude] public int SoldierEffectivenessResearchCost;
    [JsonInclude] public int SoldierSurvivabilityResearchCost;
    [JsonInclude] public int SoldierRecoverySpeedResearchCost;
    

    public const int MoneyRaisingMethodsResearchCostIncrement = 100;
    public const int TransportCapacityResearchCostIncrement = 200;
    public const int SoldierEffectivenessResearchCostIncrement = 100;
    public const int SoldierSurvivabilityResearchCostIncrement = 100;
    public const int SoldierRecoverySpeedResearchCostIncrement = 100;

    public Research()
        => Reset();

    public void Reset()
    {
        MoneyRaisingMethodsResearchCost = 500;
        TransportCapacityResearchCost = 1000;
        SoldierEffectivenessResearchCost = 500;
        SoldierSurvivabilityResearchCost = 500;
        SoldierRecoverySpeedResearchCost = 500;
    }
}