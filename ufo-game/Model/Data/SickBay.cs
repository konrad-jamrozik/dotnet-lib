using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class SickBay
{
    private const float AgentRecoverySpeedImprovement = 0.25f;

    [JsonInclude] public float AgentRecoverySpeed { get; private set; }

    public SickBay() 
        => Reset();

    public void Reset()
    {
        AgentRecoverySpeed = 0.5f;
    }

    public void ImproveAgentRecoverySpeed()
        => AgentRecoverySpeed += AgentRecoverySpeedImprovement;
}