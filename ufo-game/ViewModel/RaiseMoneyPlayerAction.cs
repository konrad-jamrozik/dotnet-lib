using UfoGame.Model;

namespace UfoGame.ViewModel;

public class RaiseMoneyPlayerAction : IPlayerActionOnButton
{
    private readonly Timeline _timeline;
    private readonly PlayerScore _playerScore;
    private readonly Accounting _accounting;

    public RaiseMoneyPlayerAction(Timeline timeline, PlayerScore playerScore, Accounting accounting)
    {
        _timeline = timeline;
        _playerScore = playerScore;
        _accounting = accounting;
    }

    public bool CanAct() => !_playerScore.GameOver;

    public void Act() => _timeline.AdvanceTime(raisedMoney: true);

    public string ActLabel() => $"Raise money: {_accounting.Data.MoneyRaisedPerActionAmount}";
}