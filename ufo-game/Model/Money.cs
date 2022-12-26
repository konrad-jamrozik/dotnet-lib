namespace UfoGame.Model;

public class Money
{
    public int CurrentMoney { get; private set; } = 0;

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