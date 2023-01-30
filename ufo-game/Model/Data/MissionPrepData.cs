using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class MissionDeploymentData : IPersistable, IResettable
{
    [JsonInclude] public int TransportCapacity { get; private set; }

    public readonly int TransportCapacityImprovement = 2;

    public MissionDeploymentData()
        => Reset();

    public void ImproveTransportCapacity()
        => TransportCapacity += TransportCapacityImprovement;

    public void Reset()
    {
        TransportCapacity = 8;
    }
}