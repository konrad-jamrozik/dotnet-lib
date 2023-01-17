﻿using System.Text.Json.Serialization;

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
    [JsonInclude] public float SoldierRecoverySpeed { get; private set; }

    [JsonInclude] public List<Soldier> Soldiers = new List<Soldier>();

    [JsonIgnore]
    public List<Soldier> AvailableSoldiers => Soldiers.Where(s => s.Available).ToList();

    public List<Soldier> AvailableSoldiersSortedByLaunchPriority(int currentTime)
        => AvailableSoldiers
            // First list soldiers that can be sent on a mission.
            .OrderByDescending(s => s.CanSendOnMission)
            // Then sort by recovery - all soldiers that can be sent have recovery 0,
            // but those who cannot will be sorted in increasing remaining recovery need.
            .ThenBy(s => s.Recovery)
            // Then sort soldiers by exp ascending
            // (rookies to be sent first, hence listed  first).
            .ThenBy(s => s.ExperienceBonus(currentTime))
            // Then from soldiers of the same experience bonus, first list
            // the ones hired more recently.
            .ThenByDescending(s => s.Id)
            .ToList();

    public List<Soldier> AssignableSoldiersSortedByLaunchPriority(int currentTime)
        => AvailableSoldiersSortedByLaunchPriority(currentTime)
            .Where(s => !s.AssignedToMission)
            .ToList();

    public List<Soldier> AssignedSoldiersSortedByDescendingLaunchPriority(int currentTime)
        => AvailableSoldiersSortedByLaunchPriority(currentTime)
            .Where(s => s.AssignedToMission)
            .Reverse()
            .ToList();

    public int SoldiersAssignedToMissionCount => Soldiers.Count(s => s.AssignedToMission);

    [JsonIgnore]
    public List<Soldier> SoldiersAssignedToMission
        => Soldiers.Where(s => s.AssignedToMission).ToList();

    [JsonIgnore]
    public List<Soldier> SoldiersInRecovery
        => Soldiers.Where(s => s.IsRecovering).ToList();

    [JsonIgnore]
    public int SoldiersInRecoveryCount
        => Soldiers.Count(s => s.IsRecovering);


    public int SoldiersSendableOnMissionCount
        => Soldiers.Count(s => s.CanSendOnMission);

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
        SoldierRecoverySpeed = 0.5f;
        Soldiers = new List<Soldier>();
    }

    public void AdvanceTime()
    {
        SoldiersInRecovery.ForEach(s => s.TickRecovery(SoldierRecoverySpeed));
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