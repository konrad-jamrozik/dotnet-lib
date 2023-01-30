using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class StaffData : IPersistable, IResettable
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