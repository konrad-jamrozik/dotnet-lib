using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionEventLogsData : IPersistable, IResettable
{
    [JsonInclude] public List<MissionEventLogData> Data = new List<MissionEventLogData>();

    public void Reset()
    {
        Data = new List<MissionEventLogData>();
    }
}