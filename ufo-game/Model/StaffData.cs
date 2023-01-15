﻿using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class StaffData
{
    public const float SoldierRecoverySpeedImprovement = 0.25f;

    [JsonInclude] public int NextSoldierId;
    [JsonInclude] public int SoldierEffectiveness;
    [JsonInclude] public int SoldierSurvivability;
    [JsonInclude] public int SoldiersToHire;
    [JsonInclude] public int SoldiersToFire;
    [JsonInclude] public int CurrentSoldiers;
    [JsonInclude] public float RecoveringSoldiers { get; private set; }
    [JsonInclude] public float SoldierRecoverySpeed { get; private set; }

    [JsonInclude] public List<Soldier> Soldiers = new List<Soldier>();

    [JsonIgnore]
    public List<Soldier> AvailableSoldiers => Soldiers.Where(s => !s.MissingInAction).ToList();

    public int SoldiersAssignedToMissionCount => Soldiers.Count(s => s.AssignedToMission);

    [JsonIgnore]
    public List<Soldier> SoldiersAssignedToMission
        => Soldiers.Where(s => s.AssignedToMission).ToList();

    [JsonIgnore]
    public List<Soldier> SoldiersInRecovery
        => Soldiers.Where(s => s.IsRecovering).ToList();

    // kja obsolete
    public int ReadySoldiers => CurrentSoldiers - (int)Math.Ceiling(RecoveringSoldiers);

    // kja obsolete
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
        NextSoldierId = 0;
        SoldierEffectiveness = 100;
        SoldierSurvivability = 100;
        SoldiersToHire = 1;
        SoldiersToFire = 1;
        CurrentSoldiers = 0;
        RecoveringSoldiers = 0;
        SoldierRecoverySpeed = 0.5f;
        Soldiers = new List<Soldier>();
    }

    public void AdvanceTime()
    {
        SoldiersInRecovery.ForEach(s => s.TickRecovery(SoldierRecoverySpeed));
        // kja obsolete
        RecoveringSoldiers = Math.Max(0, RecoveringSoldiers - SoldierRecoverySpeed);
    }

    public void HireSoldiers(int currentTime)
    {
        Enumerable.Range(NextSoldierId, SoldiersToHire)
            .ToList()
            .ForEach(
                id => Soldiers.Add(
                    new Soldier(
                        id,
                        SoldierNames.RandomName(),
                        currentTime)));
        NextSoldierId += SoldiersToHire;
    }
}