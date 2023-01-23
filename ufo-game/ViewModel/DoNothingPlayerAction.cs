using UfoGame.Model;

namespace UfoGame.ViewModel;

public class DoNothingPlayerAction : IPlayerActionOnButton
{
    private readonly Timeline _timeline;
    private readonly PlayerScore _playerScore;

    public DoNothingPlayerAction(Timeline timeline, PlayerScore playerScore)
    {
        _timeline = timeline;
        _playerScore = playerScore;
    }

    public bool CanAct() => !_playerScore.GameOver;

    public void Act() => _timeline.AdvanceTime();

    public string ActLabel() => "Do nothing & advance time";
}