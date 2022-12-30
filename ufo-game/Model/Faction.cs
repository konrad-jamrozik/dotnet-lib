namespace UfoGame.Model;

public class Faction
{
    public readonly string Name;
    public int Score;
    public readonly int ScoreTick;
    public bool Discovered { get; set; } = false;
    public bool Defeated => Score <= 0;

    public Faction(string name, int score, int scoreTick)
    {
        Name = name;
        Score = score;
        ScoreTick = scoreTick;
    }
}