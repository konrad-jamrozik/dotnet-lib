using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class Soldier
{
    [JsonInclude] public string Nickname { get; }
    [JsonInclude] public int TimeHired { get; }
    [JsonInclude] public int SuccessfulMissions { get; private set; }
    [JsonInclude] public int FailedMissions { get; private set; }
    [JsonInclude] public int TimeSpentRecovering { get; private set; }
    [JsonInclude] public float Recovery { get; private set; }
    [JsonInclude] public int TimeLost { get; private set; }

    public int ExperienceBonus(int currentTime)
        => TrainingTime(currentTime) / 2 + ExperienceFromMissions;
    
    public int Salary 
        => 5 + TotalMissions;
    
    public int TrainingTime(int currentTime)
    {
        var trainingTime = currentTime - TimeHired - TimeSpentRecovering;
        Debug.Assert(trainingTime >= 0);
        return trainingTime;
    }

    public int TotalMissions => SuccessfulMissions + FailedMissions;
    public bool CanSendOnMission => true;

    public Soldier(string nickname, int timeHired)
    {
        Nickname = nickname;
        TimeHired = timeHired;
        // kja need to archive the fact got hired
    }

    public void ReturnFromMission(bool success, float recovery)
    {
        Debug.Assert(TimeLost == 0); // dead soldiers cannot be sent on a mission
        Debug.Assert(Recovery == 0); // recovering soldiers cannot be sent on a mission
        Debug.Assert(recovery >= 0);
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
        Debug.Assert(Recovery > 0); // cannot tick recovery on ready soldier
        Recovery = Math.Max(Recovery - recovery, 0);
        TimeSpentRecovering += 1;
    }

    public void MissingInAction(int currentTime)
    {
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