using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MoneyData
{
    [JsonInclude] public int CurrentMoney { get; private set; }
    [JsonInclude] public int MoneyRaisedPerActionAmount;

    public MoneyData()
        => Reset();

    public void Reset()
    {
        CurrentMoney = 0;
        MoneyRaisedPerActionAmount = 50;
    }

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
    }

    public void SubtractMoney(int amount)
    {
        CurrentMoney -= amount;
    }
}