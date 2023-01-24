using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class ProcurementData : IDeserializable, IResettable
{
    [JsonInclude] public int AgentsToHire;
    [JsonInclude] public int AgentsToFire;

    public ProcurementData()
        => Reset();

    public void Reset()
    {
        AgentsToHire = 1;
        AgentsToFire = 1;
    }
}