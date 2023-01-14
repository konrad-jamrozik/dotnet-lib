using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Factions
{
    [JsonInclude] public List<Faction> Data = new List<Faction>();

    /// <summary>
    /// Placeholder faction for "no faction", or "null faction".
    /// By design, it has no influence on the game at all;
    /// it is  an internal implementation detail.
    /// </summary>
    /// 
    public const string NoFaction = "no_faction";

    public Factions()
        => Reset();

    public void Reset()
    {
        Data = new List<(string name, int score, int scoreTick)>
            {
                ("Strange Life Forms", 2000, 1),
                ("Zombies", 4000, 1),
                ("Black Lotus cult", 500, 10),
                ("Red Dawn remnants", 1000, 5),
                ("EXALT", 800, 15),
                ("Followers of Dagon", 1500, 5),
                ("Osiron organization", 600, 20),
                (NoFaction, 0, 0),
                // "Cult of Apocalypse", "The Syndicate",
                // "Cyberweb", "UAC", "MiB", "Hybrids", "Deep Ones"
                // // Non-canon:
                // "Man-Bear-Pigs, "Vampires", "Werewolves", "Shapeshifters",
            }
            .Select(faction => new Faction(faction.name, faction.score, faction.scoreTick))
            .ToList();
    }

    public bool AllFactionsDefeated => Data.TrueForAll(f => f.Defeated);

    public Faction RandomUndefeatedFaction
    {
        get
        {
            var undefeatedFactions = UndefeatedFactions;
            return undefeatedFactions[_random.Next(undefeatedFactions.Count)];
        }
    }

    private readonly Random _random = new Random();

    private List<Faction> UndefeatedFactions =>
        Data.Where(faction => !faction.Defeated && faction.Name != NoFaction).ToList();

    public void AdvanceFactionsTime()
    {
        foreach (var faction in Data.Where(faction => !faction.Defeated))
        {
            faction.Score += faction.ScoreTick;
        }
    }
}