using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchTransportCapacityPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;
    private readonly MissionDeployment _missionDeployment;

    public ResearchTransportCapacityPlayerAction(Research research, MissionDeployment missionDeployment)
    {
        _research = research;
        _missionDeployment = missionDeployment;
    }

    public bool CanAct() 
        => _research.CanResearchTransportCapacity();

    public void Act() 
        => _research.ResearchTransportCapacity();

    public string ActLabel()
        => $"Increase transport capacity to " +
           $"{_missionDeployment.Data.TransportCapacity + _missionDeployment.Data.TransportCapacityImprovement} " +
           $"for {_research.Data.TransportCapacityResearchCost}";
}