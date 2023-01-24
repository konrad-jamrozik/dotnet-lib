using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class FactionData
{
    [JsonInclude] public string Name { get; private set; }

    [JsonInclude]
    public int Score
    {
        get => _score;
        set => _score = Math.Max(value, 0);
    }
    private int _score;

    [JsonInclude] public int ScoreTick { get; private set; }

    [JsonInclude] public bool Discovered { get; set; } = false;

    public bool Defeated => Score <= 0;

    public FactionData(string name, int score, int scoreTick, bool discovered = false)
    {
        Name = name;
        Score = score;
        ScoreTick = scoreTick;
        Discovered = discovered;
    }
}