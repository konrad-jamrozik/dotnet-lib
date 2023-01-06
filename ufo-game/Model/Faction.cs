using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Faction
{
    public string Name { get; }

    public int Score
    {
        get => _score;
        set => _score = Math.Max(value, 0);
    }

    public int ScoreTick { get; }
    private int _score;
    public bool Discovered { get; set; } = false;

    [JsonIgnore]
    public bool Defeated => Score <= 0;

    public Faction(string name, int score, int scoreTick, bool discovered = false)
    {
        Name = name;
        Score = score;
        ScoreTick = scoreTick;
        Discovered = discovered;
    }
}