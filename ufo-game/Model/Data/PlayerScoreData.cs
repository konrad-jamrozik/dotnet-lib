﻿using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class PlayerScoreData
{
    [JsonInclude] public int Value;

    public PlayerScoreData()
        => Reset();

    public void Reset()
    {
        Value = 100;
    }
}