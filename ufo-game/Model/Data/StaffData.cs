using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class StaffData
{
    [JsonInclude] public int AgentEffectiveness;
    [JsonInclude] public int AgentSurvivability;
    
    public StaffData()
        => Reset();

    public void Reset()
    {
        AgentEffectiveness = 100;
        AgentSurvivability = 100;
    }
}