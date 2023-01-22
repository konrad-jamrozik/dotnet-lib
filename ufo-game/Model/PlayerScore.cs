using UfoGame.Model.Data;

namespace UfoGame.Model;

public class PlayerScore
{
    public readonly PlayerScoreData Data;

    public bool GameOver
        => Data.Value <= 0 || _factions.AllFactionsDefeated || _accounting.PlayerIsBroke;

    public bool PlayerWon => GameOver && Data.Value > 0 && !_accounting.PlayerIsBroke;

    public const int WinScore = 200;
    public const int LoseScore = 20;
    public const int IgnoreMissionScoreLoss = 10;

    private readonly Factions _factions;
    private readonly Accounting _accounting;

    public PlayerScore(PlayerScoreData data, Factions factions, Accounting accounting)
    {
        Data = data;
        _factions = factions;
        _accounting = accounting;
    }
}
