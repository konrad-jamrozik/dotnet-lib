using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Money
{
    [JsonInclude]
    public int CurrentMoney { get; private set; }

    [JsonInclude]
    public int MoneyRaisedPerActionAmount = 50;
    // Currently zero, as it offsets costs of actions, resulting in confusing
    // balance.
    public const int MoneyPerTurnAmount = 0;

    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
        Console.Out.WriteLine($"Added {amount} money. Money now at {CurrentMoney}");
    }

    public void SubtractMoney(int amount)
    {
        CurrentMoney -= amount;
        Console.Out.WriteLine($"Subtracted {amount} money. Money now at {CurrentMoney}");
    }
}