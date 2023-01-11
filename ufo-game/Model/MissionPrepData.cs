using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrepData
{
    [JsonInclude] public int SoldiersToSend { get; set; }

    public MissionPrepData()
        => Reset();

    public void Reset()
    {
        SoldiersToSend = 1;
    }
}