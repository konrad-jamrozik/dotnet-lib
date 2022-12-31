namespace UfoGame.Model;

public class Staff
{
    // kja sort out story with setters/getters.
    public int CurrentSoldiers { get; private set; } = 0;

    public int SoldierPrice { get; private set; } = 30;

    public int SoldierEffectiveness = 100;

    public int SoldierSurvivability = 100;

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

