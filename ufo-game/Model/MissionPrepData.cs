using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class MissionPrepData
{
    public const int TransportCapacityImprovement = 2;

    [JsonInclude] public int TransportCapacity { get; private set; }

    public void ImproveTransportCapacity()
        => TransportCapacity += TransportCapacityImprovement;

    public MissionPrepData()
        => Reset();

    public void Reset()
    {
        TransportCapacity = 4;
    }
}