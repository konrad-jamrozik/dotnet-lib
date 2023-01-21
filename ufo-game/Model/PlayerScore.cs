using System.Text.Json.Serialization;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class PlayerScore
{
    [JsonInclude]
    public readonly PlayerScoreData Data;

    public bool GameOver
        => Data.Value <= 0 || _factions.AllFactionsDefeated || _money.PlayerIsBroke;

    public bool PlayerWon => GameOver && Data.Value > 0 && !_money.PlayerIsBroke;

    public const int WinScore = 200;
    public const int LoseScore = 20;
    public const int IgnoreMissionScoreLoss = 10;

    private readonly Factions _factions;
    private readonly Money _money;

    public PlayerScore(PlayerScoreData data, Factions factions, Money money)
    {
        Data = data;
        _factions = factions;
        _money = money;
    }
}
