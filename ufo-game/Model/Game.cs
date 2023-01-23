using UfoGame.ViewModel;

namespace UfoGame.Model;

// kja rename "Game" class to "Model" and move it to ViewModel, as a hook point for UI
// to access logic. Remove all nontrivial logic from it, pushing down to relevant classes.
public class Game
{
    public readonly Timeline Timeline;
    public readonly PendingMission PendingMission;
    public readonly PlayerScore PlayerScore;
    public readonly StateRefresh StateRefresh;

    public Game(
        Timeline timeline,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        PlayerScore playerScore)
    {
        Timeline = timeline;
        PendingMission = pendingMission;
        StateRefresh = stateRefresh;
        PlayerScore = playerScore;
    }

    public bool CanDoNothing() => !PlayerScore.GameOver;

    public void DoNothing() => AdvanceTime();

    public bool CanRaiseMoney() => !PlayerScore.GameOver;

    public void RaiseMoney()
        => AdvanceTime(raisedMoney: true);

    private void AdvanceTime(bool raisedMoney = false)
        => Timeline.AdvanceTime(raisedMoney);
}
