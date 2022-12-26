namespace UfoGame.Model;

public class GameState
{
    public readonly Timeline Timeline;
    public readonly Money Money;

    public GameState(Timeline timeline, Money money)
    {
        Timeline = timeline;
        Money = money;
    }

    public void AdvanceTime()
    {
        Timeline.IncrementTime();
        Money.AddMoney(10);
    }
}
