using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionEventLogData
{
    [JsonInclude] public string Summary { get; private set; }
    [JsonInclude] public string? Details { get; private set; }

    public MissionEventLogData(string summary, string? details = null)
    {
        Summary = summary;
        Details = details;
    }
}