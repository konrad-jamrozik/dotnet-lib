namespace UfoGameLib;

public record Assets(int CurrentMoney, Agents Agents)
{
    public int CurrentMoney { get; set; } = CurrentMoney;
    public Agents Agents { get; set; } = Agents;
}