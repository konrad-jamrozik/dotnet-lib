using UfoGame.Model.Data;

namespace UfoGame.Model;

public class PlayerScore
{
    public readonly PlayerScoreData Data;

    public bool GameOver
        => Data.Value <= 0 || _factionsData.AllFactionsDefeated || _accounting.PlayerIsBroke;

    public bool PlayerWon => GameOver && Data.Value > 0 && !_accounting.PlayerIsBroke;

    public const int WinScore = 200;
    public const int LoseScore = 20;
    public const int IgnoreMissionScoreLoss = 10;

    private readonly FactionsData _factionsData;
    private readonly Accounting _accounting;

    public PlayerScore(PlayerScoreData data, FactionsData factionsData, Accounting accounting)
    {
        Data = data;
        _factionsData = factionsData;
        _accounting = accounting;
    }
}
