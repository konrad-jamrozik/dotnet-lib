namespace UfoGame.Model;

public class GameState
{
    public readonly Timeline Timeline;
    public readonly Money Money;
    public readonly Staff Staff;

    public GameState(Timeline timeline, Money money, Staff staff)
    {
        Timeline = timeline;
        Money = money;
        Staff = staff;
    }

    public void AdvanceTime()
    {
        Timeline.IncrementTime();
        Money.AddMoney(10);
    }

    public void HireSoldier()
    {
        Money.SubtractMoney(Staff.SoldierPrice);
        Staff.HireSoldier();
    }

    public bool CanHireSoldier()
    {
        return Money.CurrentMoney >= Staff.SoldierPrice;
    }
}
