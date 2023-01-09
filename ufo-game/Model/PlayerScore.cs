using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PlayerScore
{
    public bool GameOver => Value <= 0 || _factions.AllFactionsDefeated;
    public const int WinScore = 500;
    public const int LoseScore = 100;
    public const int IgnoreMissionScoreLoss = 50;

    [JsonInclude]
    public int Value { get; set; } = 1000;

    private readonly Factions _factions;

    public PlayerScore(Factions factions)
    {
        _factions = factions;
    }
}
