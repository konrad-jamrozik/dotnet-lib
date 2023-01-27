using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionDeploymentData : IPersistable, IResettable
{
    public readonly int TransportCapacityImprovement = 2;

    [JsonInclude] public int TransportCapacity { get; private set; }

    public void ImproveTransportCapacity()
        => TransportCapacity += TransportCapacityImprovement;

    public MissionDeploymentData()
        => Reset();

    public void Reset()
    {
        TransportCapacity = 8;
    }
}