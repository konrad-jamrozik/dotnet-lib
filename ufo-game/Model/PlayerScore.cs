namespace UfoGame.Model;

public class PlayerScore
{
    public const int WinScore = 500;
    public const int LoseScore = 100;
    public const int IgnoreMissionScoreLoss = 50;
    public int Value { get; set; } = 1000;
}
