using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

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

    public int ExperienceBonus(int currentTime)
    {
        Debug.Assert(currentTime >= TimeHired);
        return TrainingTime(currentTime) + ExperienceFromMissions;
    }

    // kja hook it up to AgentListItem UI instead of Recovery
    public int TimeToRecover(float recoverySpeed) => (int)Math.Ceiling(Recovery / recoverySpeed);

    public int Salary 
        => 5 + TotalMissions;
    // kja need to dedup salary with UfoGame.Model.Money.Expenses
    
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
        // kja need to archive the fact got hired
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

    private readonly int[] _missionExperienceBonus = 
    {
        60, 50, 40, 30, 20 // Sum: 200
    };

    /// <summary>
    /// Agent that was on N missions gets sum of experience from
    /// _missionExperienceBonus array, from 0th element to Nth element.
    /// For each mission above N, the agents gets additional amount
    /// of experience equal to the last value in _missionExperienceBonus.
    /// For example, if agent was on N+K missions, where the array
    /// is of length N, they will have experience bonus equal to the
    /// sum of all values of the array, plus the last value of the array times K.
    /// </summary>
    private int ExperienceFromMissions =>
        _missionExperienceBonus.Take(TotalMissions).Sum()
        + (Math.Max(TotalMissions - _missionExperienceBonus.Length, 0) *
        _missionExperienceBonus[^1]);
}