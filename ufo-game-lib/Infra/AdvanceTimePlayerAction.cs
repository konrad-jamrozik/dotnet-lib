namespace UfoGameLib.Infra;

public class AdvanceTimePlayerAction : PlayerAction
{
    public override void Apply(GameState state)
    {
        state.Timeline.CurrentTurn++;
        state.Assets.CurrentMoney -= state.Assets.Agents.Count * 5;
    }
}