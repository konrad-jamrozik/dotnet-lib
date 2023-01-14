using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Soldier
{
    // kja add ID to UI
    [JsonInclude] public int Id { get; private set; }
    [JsonInclude] public string Nickname { get; private set; }
    [JsonInclude] public int TimeHired { get; private set; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int TimeSpentRecovering { get; private set; }
    [JsonInclude] public float Recovery { get; private set; }
    [JsonInclude] public int TimeLost { get; private set; }

    public int ExperienceBonus(int currentTime)
    {
        Debug.Assert(currentTime >= TimeHired);
        return TrainingTime(currentTime) / 2 + ExperienceFromMissions;
    }

    // kja hook it up to SoldierListItem UI instead of Recovery
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
    public bool CanSendOnMission => !MissingInAction && !IsRecovering;
    public bool MissingInAction => TimeLost != 0;
    public bool IsRecovering => Recovery > 0;

    public Soldier(int id, string nickname, int timeHired)
    {
        Id = id;
        Nickname = nickname;
        TimeHired = timeHired;
        // kja need to archive the fact got hired
    }

    public void ReturnFromMission(bool success, float recovery)
    {
        Debug.Assert(recovery >= 0);
        Debug.Assert(!MissingInAction); // dead soldiers cannot be sent on a mission
        Debug.Assert(!IsRecovering); // recovering soldiers cannot be sent on a mission
        
        if (success)
        {
            SuccessfulMissions += 1;
        }
        else
        {
            FailedMissions += 1;
        }
        Recovery += recovery;
    }

    public void TickRecovery(float recovery)
    {
        Debug.Assert(recovery >= 0);
        Debug.Assert(!IsRecovering); // cannot tick recovery on ready soldier
        Recovery = Math.Max(Recovery - recovery, 0);
        TimeSpentRecovering += 1;
    }

    public void RecordLost(int currentTime)
    {
        Debug.Assert(currentTime >= TimeHired);
        Debug.Assert(!MissingInAction);
        Debug.Assert(!IsRecovering);
        TimeLost = currentTime;
        // kja need to archive the fact they are missing in action
    }

    private readonly int[] _missionExperienceBonus = 
    {
        0, 30, 25, 20, 15, 10, 5
    };

    /// <summary>
    /// Soldier that was on N missions gets sum of experience from
    /// _missionExperienceBonus array, from 0th element to Nth element.
    /// For each mission above N, the soldiers gets additional amount
    /// of experience equal to the last value in _missionExperienceBonus.
    /// For example, if soldier was on N+K missions, where the array
    /// is of length N, they will have experience bonus equal to the
    /// sum of all values of the array, plus the last value of the array times K.
    /// </summary>
    private int ExperienceFromMissions =>
        _missionExperienceBonus.Take(TotalMissions).Sum()
        + Math.Max(TotalMissions - (_missionExperienceBonus.Length - 1), 0) *
        _missionExperienceBonus[^1];
}