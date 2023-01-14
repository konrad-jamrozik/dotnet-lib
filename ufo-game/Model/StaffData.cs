﻿using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class StaffData
{
    public const float SoldierRecoverySpeedImprovement = 0.25f;

    [JsonInclude] public int SoldierEffectiveness;
    [JsonInclude] public int SoldierSurvivability;
    [JsonInclude] public int SoldiersToHire;
    [JsonInclude] public int CurrentSoldiers;
    [JsonInclude] public float RecoveringSoldiers { get; private set; }
    [JsonInclude] public float SoldierRecoverySpeed { get; private set; }

    public int ReadySoldiers => CurrentSoldiers - (int)Math.Ceiling(RecoveringSoldiers);

    public void AddRecoveringSoldiers(int count)
    {
        Debug.Assert(RecoveringSoldiers + count <= CurrentSoldiers);
        RecoveringSoldiers += count;
    }

    public void ImproveSoldierRecoverySpeed()
        => SoldierRecoverySpeed += SoldierRecoverySpeedImprovement;

    public StaffData()
        => Reset();

    public void Reset()
    {
        SoldierEffectiveness = 100;
        SoldierSurvivability = 100;
        SoldiersToHire = 1;
        CurrentSoldiers = 0;
        RecoveringSoldiers = 0;
        SoldierRecoverySpeed = 0.5f;
    }

    public void AdvanceTime()
    {
        RecoveringSoldiers = Math.Max(0, RecoveringSoldiers - SoldierRecoverySpeed);
    }
}