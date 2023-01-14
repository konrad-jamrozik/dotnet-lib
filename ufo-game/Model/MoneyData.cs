using System.Text.Json.Serialization;

namespace UfoGame.Model;

// kja maybe all "Data" classes could become private classes (thus enabling strong access protection)
// Hopefully this means they can still be json-serialized.
// I would do manual obj graph construction and then add all classes to the DI container to make
// Blazor framework happy.
public class MoneyData
{
    [JsonInclude] public int CurrentMoney { get; set; }
    [JsonInclude] public int MoneyRaisedPerActionAmount;

    public MoneyData()
        => Reset();

    public void Reset()
    {
        CurrentMoney = 0;
        MoneyRaisedPerActionAmount = 100;
    }
}