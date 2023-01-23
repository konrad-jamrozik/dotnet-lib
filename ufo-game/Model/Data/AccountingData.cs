﻿using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class AccountingData
{
    [JsonInclude] public int CurrentMoney { get; set; }
    [JsonInclude] public int MoneyRaisedPerActionAmount;

    public AccountingData()
        => Reset();

    public void Reset()
    {
        CurrentMoney = 0;
        MoneyRaisedPerActionAmount = 100;
    }
}