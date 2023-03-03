using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionEventLogData
{
    [JsonInclude] public string Description { get; private set; }
    [JsonInclude] public string? Details { get; private set; }

    public MissionEventLogData(string description, string? details = null)
    {
        Description = description;
        Details = details;
    }
}