using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class Agent
{
    [JsonInclude] public int Id { get; private set; }
    [JsonInclude] public string Nickname { get; private set; }
    [JsonInclude] public int TimeHired { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int TimeSpentRecovering { get; private set; }
    [JsonInclude] public float Recovery { get; private set; }
    [JsonInclude] public int TimeLost { get; private set; }
    [JsonInclude] public bool AssignedToMission { get; private set; }

    // kja Agent: inject TimelineData instead of taking currentTime as param; this will likely require extracting AgentData

    public int ExperienceBonus(int currentTime) 
    {
        Debug.Assert(currentTime >= TimeHired);
        return TrainingTime(currentTime) + ExperienceFromMissions;
    }

    public int TimeToRecover(float recoverySpeed) => (int)Math.Ceiling(Recovery / recoverySpeed);

    public int Salary => 5 + TotalMissions;
    
    public int TrainingTime(int currentTime)
    {
        Debug.Assert(currentTime >= TimeHired);
        var trainingTime = currentTime - TimeHired - TimeSpentRecovering;
        Debug.Assert(trainingTime >= 0);
        return trainingTime;
    }

    public int TimeEmployed(int currentTime)
    {
        Debug.Assert(currentTime >= TimeHired);
        int timeEmployed;
        if (MissingInAction)
        {
            Debug.Assert(TimeLost <= currentTime);
            timeEmployed = TimeLost - TimeHired;
        }
        else
        {
            timeEmployed =  currentTime - TimeHired;
        }
        Debug.Assert(timeEmployed >= 0);
        return timeEmployed;
    }

    public int TotalMissions => SuccessfulMissions + FailedMissions;
    public bool MissingInAction => TimeLost != 0;
    public bool Available => !MissingInAction;
    public bool IsRecovering => !MissingInAction && Recovery > 0;
    public bool IsAtFullHealth => !MissingInAction && !IsRecovering;
    public bool CanSendOnMission => IsAtFullHealth;
    public bool IsAssignableToMission => CanSendOnMission && !AssignedToMission;
    public bool IsUnassignableFromMission => IsAtFullHealth && AssignedToMission;
    public bool CouldHaveBeenSentOnMission => IsAtFullHealth;

    public Agent(int id, string nickname, int timeHired)
    {
        Id = id;
        Nickname = nickname;
        TimeHired = timeHired;
    }

    public void AssignToMission()
    {
        Debug.Assert(IsAssignableToMission);
        AssignedToMission = true;
    }

    public void UnassignFromMission()
    {
        Debug.Assert(IsUnassignableFromMission);
        AssignedToMission = false;
    }

    public void RecordMissionOutcome(bool success, float recovery)
    {
        Debug.Assert(recovery >= 0);
        Debug.Assert(CouldHaveBeenSentOnMission);
        
        if (success)
        {
            SuccessfulMissions += 1;
        }
        else
        {
            FailedMissions += 1;
        }

        if (recovery > 0)
            UnassignFromMission();

        Recovery += recovery;
    }

    public void TickRecovery(float recovery)
    {
        Debug.Assert(recovery >= 0);
        Debug.Assert(IsRecovering); // cannot tick recovery on a non-recovering agent.
        Recovery = Math.Max(Recovery - recovery, 0);
        TimeSpentRecovering += 1;
    }

    public void SetAsLost(int currentTime, bool missionSuccess)
    {
        Debug.Assert(currentTime >= TimeHired);
        Debug.Assert(IsAtFullHealth);
        Debug.Assert(AssignedToMission);
        RecordMissionOutcome(missionSuccess, recovery: 0);
        UnassignFromMission();
        TimeLost = currentTime;
    }

    // As of 1/19/2023 worst case possible on successful mission is that agent will need
    // PendingMission.MaxAgentSurvivalChance / 2 recovery units at recovery speed of 1.
    // This is 99 / 2 = 49.
    // Because agents train 1 experience per turn, this means that going on 
    // a successful mission always makes the agent improve faster,
    // because in the worst case an agent will miss 49 days of training
    // but gain 50 experience, so ahead by 1.
    // Note that this is true only if recovery speed is 1 or more, and currently it starts at 0.5.
    private readonly int[] _missionExperienceBonus = 
    {
        150, 125, 100, 75, 50 // Sum: 500
    };

    /// <summary>
    /// Agent that was on N missions gets sum of experience from
    /// _missionExperienceBonus array, from 0th element to (N-1)th element.
    /// For each mission above N-1, the agents gets additional amount
    /// of experience equal to the last value in _missionExperienceBonus.
    ///
    /// Example 1: if agent was on N+K missions, where the array
    /// is of length N, they will have experience bonus from missions
    /// equal to the sum of all values of the array except the last value,
    /// plus the last value of the array times K.
    ///
    /// Example 2: If the array values are {5,3,1} and the agent was
    /// on 6 missions, their experience from mission will be a sum of:
    /// 5,3,1,1,1,1, which is 12.
    /// 
    /// </summary>
    private int ExperienceFromMissions =>
        _missionExperienceBonus.Take(TotalMissions).Sum()
        + (Math.Max(TotalMissions - _missionExperienceBonus.Length, 0) *
        _missionExperienceBonus[^1]);
}