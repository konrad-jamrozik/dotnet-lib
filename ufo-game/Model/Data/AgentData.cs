using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class AgentData
{
    [JsonInclude] public int Id { get; private set; }
    [JsonInclude] public string FullName { get; private set; }
    [JsonInclude] public int TimeHired { get; private set; }
    [JsonInclude] public int SuccessfulMissions;
    [JsonInclude] public int FailedMissions;
    [JsonInclude] public int TimeSpentRecovering;
    [JsonInclude] public float Recovery;
    [JsonInclude] public int TimeLost;
    [JsonInclude] public int TimeSacked;
    [JsonInclude] public bool AssignedToMission;

    public int TimeLostOrSacked => Math.Max(TimeLost, TimeSacked);

    public AgentData(int id, string fullName, int timeHired)
    {
        Id = id;
        FullName = fullName;
        TimeHired = timeHired;
    }
}