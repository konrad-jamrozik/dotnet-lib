using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Factions
{
    public List<Faction> Data { get; }

    public Factions(List<Faction>? data = null)
    {
        Data = data ?? new List<(string name, int score, int scoreTick)>
            {
                ("Strange Life Forms", 5000, 1),
                ("Zombies", 10000, 1),
                ("Black Lotus cult", 1000, 10),
                ("Red Dawn remnants", 2000, 5),
                ("EXALT", 1000, 15),
                ("Followers of Dagon", 2000, 5),
                ("Osiron organization", 500, 20),
                // "Cult of Apocalypse", "The Syndicate",
                // "Cyberweb", "UAC", "MiB", "Hybrids", "Deep Ones"
                // // Non-canon:
                // "Man-Bear-Pigs, "Vampires", "Werewolves", "Shapeshifters",
            }
            .Select(faction => new Faction(faction.name, faction.score, faction.scoreTick))
            .ToList();
    }

    [JsonIgnore]
    public bool AllFactionsDefeated => Data.TrueForAll(f => f.Defeated);

    public void AdvanceFactionsTime()
    {
        foreach (var faction in Data.Where(faction => !faction.Defeated))
        {
            faction.Score += faction.ScoreTick;
        }
    }
}