using System.Data;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Money
{
    [JsonInclude]
    public readonly MoneyData Data;

    private readonly OperationsArchive _archive;
    private readonly StaffData _staffData;

    public int PassiveIncome =>
        80
        + _archive.SuccessfulMissions * 40
        + _archive.FailedMissions * -10
        + _archive.IgnoredMissions * -5;

    public int Expenses => _staffData.CurrentSoldiers * 10;

    public int MoneyPerTurnAmount => PassiveIncome - Expenses;

    public Money(MoneyData data, OperationsArchive archive, StaffData staffData)
    {
        Data = data;
        _archive = archive;
        _staffData = staffData;
    }

    public int CurrentMoney => Data.CurrentMoney;

    public bool PlayerIsBroke => CurrentMoney < 0;

    public void AddMissionLoot(int amount)
        => Data.CurrentMoney += amount;

    public void PayForHiringSoldiers(int cost)
        => Data.CurrentMoney -= cost;

    public void PayForResearch(int cost)
        => Data.CurrentMoney -= cost;

    public void AdvanceTime(bool raisedMoney = false)
    {
        Data.CurrentMoney +=
            MoneyPerTurnAmount + (raisedMoney ? Data.MoneyRaisedPerActionAmount : 0);
    }
}