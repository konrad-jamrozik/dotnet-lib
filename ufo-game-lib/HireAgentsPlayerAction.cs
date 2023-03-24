namespace UfoGameLib;

public class HireAgentsPlayerAction : PlayerAction
{
    public int Count { get; }

    public HireAgentsPlayerAction(int count)
    {
        Count = count;
    }
}