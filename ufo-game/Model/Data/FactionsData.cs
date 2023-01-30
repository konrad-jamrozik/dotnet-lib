using System.Text.Json.Serialization;
using UfoGame.Infra;

namespace UfoGame.Model.Data;

public class FactionsData : ITemporal, IPersistable, IResettable
{
    /// <summary>
    /// Placeholder faction for "no faction", or "null faction".
    /// By design, it has no influence on the game at all;
    /// it is  an internal implementation detail.
    /// </summary>
    /// 
    public const string NoFaction = "no_faction";

    [JsonInclude] public List<FactionData> Data = new List<FactionData>();

    public FactionsData()
        => Reset();

    public bool AllFactionsDefeated => Data.TrueForAll(f => f.Defeated);

    public FactionData RandomUndefeatedFactionData(RandomGen randomGen)
    {
        var undefeatedFactions = UndefeatedFactions;
        return undefeatedFactions[randomGen.Random.Next(undefeatedFactions.Count)];
    }

    private List<FactionData> UndefeatedFactions =>
        Data.Where(faction => !faction.Defeated && faction.Name != NoFaction).ToList();

    public void Reset()
    {
        Data = new List<(string name, int score, int scoreTick)>
            {
                ("Strange Life Forms", 600, 1),
                ("Zombies", 1200, 1),
                ("Black Lotus cult", 200, 5),
                ("Red Dawn remnants", 300, 5),
                ("EXALT", 400, 6),
                ("Followers of Dagon", 800, 5),
                ("Osiron organization", 400, 8),
                (NoFaction, 0, 0),
                // "Cult of Apocalypse", "The Syndicate",
                // "Cyberweb", "UAC", "MiB", "Hybrids", "Deep Ones"
                // // Non-canon:
                // "Man-Bear-Pigs, "Vampires", "Werewolves", "Shapeshifters",
            }
            .Select(faction => new FactionData(faction.name, faction.score, faction.scoreTick))
            .ToList();
    }

    public void AdvanceTime()
    {
        foreach (var faction in Data.Where(faction => !faction.Defeated))
        {
            faction.Score += faction.ScoreTick;
        }
    }
}