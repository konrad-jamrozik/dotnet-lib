using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PlayerScore
{
    [JsonInclude]
    public readonly PlayerScoreData Data;

    public bool GameOver => Data.Value <= 0 || _factions.AllFactionsDefeated;

    public bool PlayerWon => GameOver && Data.Value > 0;

    public const int WinScore = 500;
    public const int LoseScore = 100;
    public const int IgnoreMissionScoreLoss = 50;

    private readonly Factions _factions;

    public PlayerScore(PlayerScoreData data, Factions factions)
    {
        Data = data;
        _factions = factions;
    }
}
