namespace UfoGame.Model;

// kja rename "Game" class to "Model" and move it to ViewModel, as a hook point for UI
// to access logic. Remove all nontrivial logic from it, pushing down to relevant classes.
public class Game
{
    private readonly Timeline _timeline;
    private readonly PlayerScore _playerScore;

    public Game(
        Timeline timeline,
        PlayerScore playerScore)
    {
        _timeline = timeline;
        _playerScore = playerScore;
    }

    public bool CanDoNothing() => !_playerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public bool CanRaiseMoney() => !_playerScore.GameOver;

    public void RaiseMoney()
        => AdvanceTime(raisedMoney: true);

    private void AdvanceTime(bool raisedMoney = false)
        => _timeline.AdvanceTime(raisedMoney);
}
