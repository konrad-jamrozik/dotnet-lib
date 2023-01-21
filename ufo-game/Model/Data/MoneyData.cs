using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

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