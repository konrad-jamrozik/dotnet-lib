using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Accounting
{
    public readonly AccountingData Data;

    private readonly Archive _archive;
    private readonly Staff _staff;

    public int PassiveIncome =>
        80
        + _archive.SuccessfulMissions * 10
        + _archive.FailedMissions * -10
        + _archive.IgnoredMissions * -5;

    public int Expenses => _staff.Data.AvailableAgents.Sum(agent => agent.Salary);

    public int MoneyPerTurnAmount => PassiveIncome - Expenses;

    public Accounting(AccountingData data, Archive archive, Staff staff)
    {
        Data = data;
        _archive = archive;
        _staff = staff;
    }

    public int CurrentMoney => Data.CurrentMoney;

    public bool PlayerIsBroke => CurrentMoney < 0;

    public void AddMissionLoot(int amount)
        => Data.CurrentMoney += amount;

    public void PayForHiringAgents(int cost)
        => Data.CurrentMoney -= cost;

    public void PayForResearch(int cost)
        => Data.CurrentMoney -= cost;

    // kja split it by introducing "AddRaisedMoney"; then introduce ITemporal interface for AdvanceTime.
    public void AdvanceTime(bool raisedMoney = false)
        => Data.CurrentMoney +=
            MoneyPerTurnAmount + (raisedMoney ? Data.MoneyRaisedPerActionAmount : 0);
}