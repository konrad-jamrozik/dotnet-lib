using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class HireAgentsPlayerAction : PlayerAction
{
    public int Count { get; }

    public HireAgentsPlayerAction(int count)
    {
        Count = count;
    }

    public override void Apply(GameState state)
    {
        Console.Out.WriteLine($"Hire agents. Count: {Count}");
        for (int i = 0; i < Count; i++)
            state.Assets.Agents.Add(new Agent(state.NextAgentId));
    }
}