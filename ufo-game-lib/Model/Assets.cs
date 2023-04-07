namespace UfoGameLib.Model;

public record Assets(int CurrentMoney, Agents Agents, int MaxTransportCapacity, int CurrentTransportCapacity)
{
    public int CurrentMoney { get; set; } = CurrentMoney;
    public Agents Agents { get; set; } = Agents;
    public int MaxTransportCapacity { get; set; } = MaxTransportCapacity;
    public int CurrentTransportCapacity { get; set; } = CurrentTransportCapacity;

    protected Assets(Assets original)
    {
        CurrentMoney = original.CurrentMoney;
        Agents = (Agents)original.Agents.Clone();
        MaxTransportCapacity = original.MaxTransportCapacity;
        CurrentTransportCapacity = original.CurrentTransportCapacity;
    }
}