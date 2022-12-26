namespace UfoGame.Model;

public class Staff
{
    public int CurrentSoldiers { get; private set; } = 0;

    public int SoldierPrice { get; private set; } = 30;

    public void HireSoldier()
    {
        CurrentSoldiers += 1;
        Console.Out.WriteLine($"Hired solider. Soldiers now at {CurrentSoldiers}.");
    }

    public void SubtractSoldiers(int amount)
    {
        CurrentSoldiers -= amount;
        Console.Out.WriteLine($"Subtracted {amount} soldiers. Soldiers now at {CurrentSoldiers}.");
    }
}

