﻿using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Research
{
    [JsonInclude] public int MoneyRaisingMethodsResearchCost;
    [JsonInclude] public int TransportCapacityResearchCost;
    [JsonInclude] public int AgentEffectivenessResearchCost;
    [JsonInclude] public int AgentSurvivabilityResearchCost;
    [JsonInclude] public int AgentRecoverySpeedResearchCost;
    

    public const int MoneyRaisingMethodsResearchCostIncrement = 100;
    public const int TransportCapacityResearchCostIncrement = 200;
    public const int AgentEffectivenessResearchCostIncrement = 100;
    public const int AgentSurvivabilityResearchCostIncrement = 100;
    public const int AgentRecoverySpeedResearchCostIncrement = 100;

    public Research()
        => Reset();

    public void Reset()
    {
        MoneyRaisingMethodsResearchCost = 500;
        TransportCapacityResearchCost = 1000;
        AgentEffectivenessResearchCost = 500;
        AgentSurvivabilityResearchCost = 500;
        AgentRecoverySpeedResearchCost = 500;
    }
}