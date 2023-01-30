using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class ProcurementData : IPersistable, IResettable
{
    [JsonInclude] public int AgentsToHire;

    public ProcurementData()
        => Reset();

    public void Reset()
    {
        AgentsToHire = 1;
    }
}