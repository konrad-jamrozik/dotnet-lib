using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Faction
{
    private int _score;

    [JsonInclude]
    public string Name { get; private set; }

    [JsonInclude]
    public int Score
    {
        get => _score;
        set => _score = Math.Max(value, 0);
    }

    [JsonInclude]
    public int ScoreTick { get; private set; }

    [JsonInclude]
    public bool Discovered { get; set; } = false;

    public bool Defeated => Score <= 0;

    public Faction(string name, int score, int scoreTick, bool discovered = false)
    {
        Name = name;
        Score = score;
        ScoreTick = scoreTick;
        Discovered = discovered;
    }
}