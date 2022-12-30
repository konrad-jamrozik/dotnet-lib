namespace UfoGame.Model;

public class Faction
{
    public readonly string Name;
    public int Score { get; set; } = 0;
    public bool Discovered { get; set; } = false;

    public Faction(string name)
    {
        Name = name;
    }
}