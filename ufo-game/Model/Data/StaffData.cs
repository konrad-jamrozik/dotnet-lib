using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class StaffData : IDeserializable, IResettable
{
    // kja move StaffData contents to appropriate class
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