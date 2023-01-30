using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Accounting : ITemporal
{
    public readonly AccountingData Data;

    private readonly ArchiveData _archiveData;
    private readonly Agents _agents;

    public Accounting(AccountingData data, ArchiveData archiveData, Agents agents)
    {
        Data = data;
        _archiveData = archiveData;
        _agents = agents;
    }

    public int PassiveIncome =>
        80
        + _archiveData.SuccessfulMissions * 10
        + _archiveData.FailedMissions * -10
        + _archiveData.IgnoredMissions * -5;

    public int Expenses => _agents.AvailableAgents.Sum(agent => agent.Salary);

    public int MoneyPerTurnAmount => PassiveIncome - Expenses;

    public int CurrentMoney => Data.CurrentMoney;

    public bool PlayerIsBroke => CurrentMoney < 0;

    public void AddMissionLoot(int amount)
        => Data.CurrentMoney += amount;

    public void PayForHiringAgents(int cost)
        => Data.CurrentMoney -= cost;

    public void PayForResearch(int cost)
        => Data.CurrentMoney -= cost;

    public void AddRaisedMoney()
        => Data.CurrentMoney += Data.MoneyRaisedPerActionAmount;

    public void AdvanceTime()
    {
        // Console.WriteLine($"Accounting.AdvanceTime: " +
        //                   $"Data.CurrentMoney {Data.CurrentMoney} MoneyPerTurnAmount {MoneyPerTurnAmount} " +
        //                   $"PassiveIncome {PassiveIncome} Expenses {Expenses}");
        Data.CurrentMoney += MoneyPerTurnAmount;
    }
}