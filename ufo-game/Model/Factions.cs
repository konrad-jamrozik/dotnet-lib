namespace UfoGame.Model;

public class Factions
{
    public readonly List<Faction> Data;

    public Factions()
    {
        Data = new List<string>
                { "Black Lotus", "Red Dawn", "Exalt", "The Syndicate", "Cyberweb", "UAC", "MiB" }
            .Select(name => new Faction(name))
            .ToList();
    }
}