using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Research
{
    [JsonInclude]
    public int MoneyRaisingMethodsResearchCost = 100;
    public const int MoneyRaisingMethodsResearchCostIncrement = 10;

    [JsonInclude]
    public int SoldierEffectivenessResearchCost = 100;
    public const int SoldierEffectivenessResearchCostIncrement = 10;

    [JsonInclude]
    public int SoldierSurvivabilityResearchCost = 100;
    public const int SoldierSurvivabilityResearchCostIncrement = 10;   
}