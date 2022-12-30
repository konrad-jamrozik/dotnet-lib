namespace UfoGame.Model;

public class Factions
{
    public readonly List<Faction> Data;

    public Factions()
    {
        Data = new List<string>
            {
                "Strange Life Forms", "Zombies",
                "Black Lotus cult", "Red Dawn remnants", "Exalt organization", "Followers of Dagon cult", 
                "Osiron organization",
                // "Cult of Apocalypse", "The Syndicate",
                // "Cyberweb", "UAC", "MiB", "Hybrids", "Deep Ones"
                // // Non-canon:
                // "Man-Bear-Pigs, "Vampires", "Werewolves",  "Shapeshifters",
            }
            .Select(name => new Faction(name))
            .ToList();
    }
}