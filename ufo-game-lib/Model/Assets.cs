namespace UfoGameLib.Model;

public record Assets(int CurrentMoney, Agents Agents, int TransportCapacity)
{
    public int CurrentMoney { get; set; } = CurrentMoney;
    public Agents Agents { get; set; } = Agents;

    protected Assets(Assets original)
    {
        CurrentMoney = original.CurrentMoney;
        Agents = (Agents)original.Agents.Clone();
        TransportCapacity = original.TransportCapacity;
    }
}