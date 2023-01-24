using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class AgentData : IDeserializable
{
    [JsonInclude] public int Id;
    [JsonInclude] public string FullName;
    [JsonInclude] public int TimeHired;
    [JsonInclude] public int SuccessfulMissions;
    [JsonInclude] public int FailedMissions;
    [JsonInclude] public int TimeSpentRecovering;
    [JsonInclude] public float Recovery;
    [JsonInclude] public int TimeLost;
    [JsonInclude] public bool AssignedToMission;

    public AgentData(int id, string fullName, int timeHired)
    {
        Id = id;
        FullName = fullName;
        TimeHired = timeHired;
    }
}