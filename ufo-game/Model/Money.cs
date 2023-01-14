using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Money
{
    [JsonInclude]
    public readonly MoneyData Data;

    // kja passive income
    // Formula: base + missions_won*c1 - missions_lost*c2 
    // base: 60 + missions_won * 20 - missions_lost * 5
    // more advanced formula: based on mission log
    // public int PassiveIncome;
    
    // kja expenses
    // Formula: soldiers * 10
    // public int Expenses => 

    // kja to be computed based on PassiveIncome and expenses.
    // Currently zero, as it offsets costs of actions, resulting in confusing
    // balance.
    public const int MoneyPerTurnAmount = 0;

    public Money(MoneyData data)
    {
        Data = data;
    }

    public int CurrentMoney => Data.CurrentMoney;

    public int MoneyRaisedPerActionAmount
    {
        get => Data.MoneyRaisedPerActionAmount;
        set => Data.MoneyRaisedPerActionAmount = value;
    }

    public void AddMoney(int amount)
    {
        Data.AddMoney(amount);
    }

    public void SubtractMoney(int amount)
    {
        Data.SubtractMoney(amount);
    }
}