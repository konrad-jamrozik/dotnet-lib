using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrepData
{
    [JsonInclude]
    public int SoldiersToSend { get; set; }
}