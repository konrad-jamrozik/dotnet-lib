using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrepData
{
    [JsonInclude] public int SoldiersToSend { get; set; }
    [JsonInclude] public int TransportCapacity { get; private set; }

    public const int TransportCapacityImprovement = 2;

    public void ImproveTransportCapacity()
        => TransportCapacity += TransportCapacityImprovement;

    public MissionPrepData()
        => Reset();

    public void Reset()
    {
        SoldiersToSend = 1;
        TransportCapacity = 4;
    }
}