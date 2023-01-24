using UfoGame.Model.Data;

namespace UfoGame.Model;

public class SickBay : ITemporal
{
    public readonly SickBayData Data;
    private readonly Agents _agents;

    public SickBay(SickBayData data, Agents agents)
    {
        Data = data;
        _agents = agents;
    }

    public void AdvanceTime()
        => _agents.AgentsInRecovery.ForEach(agent => agent.TickRecovery(Data.AgentRecoverySpeed));
}