namespace UfoGameLib.Infra;

public class AdvanceTimePlayerAction : PlayerAction
{
    public override void Apply(GameState state)
    {
        Console.Out.WriteLine($"AdvanceTimePlayerAction.Apply CurrentTurn: {state.Timeline.CurrentTurn}");
        state.Timeline.CurrentTurn++;
        state.Assets.CurrentMoney -= state.Assets.Agents.Count * 5;
    }
}