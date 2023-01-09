using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Money
{
    [JsonInclude]
    public int CurrentMoney { get; private set; }

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