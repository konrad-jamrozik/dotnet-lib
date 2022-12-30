namespace UfoGame.Model;

public class Faction
{
    public readonly string Name;

    public int Score
    {
        get => _score;
        set => _score = Math.Max(value, 0);
    }

    public readonly int ScoreTick;
    private int _score;
    public bool Discovered { get; set; } = false;
    public bool Defeated => Score <= 0;

    public Faction(string name, int score, int scoreTick)
    {
        Name = name;
        Score = score;
        ScoreTick = scoreTick;
    }
}