﻿using UfoGame.Model;

namespace UfoGame.ViewModel;

public class ResearchTransportCapacityPlayerAction : IPlayerActionOnButton
{
    private readonly Research _research;
    private readonly MissionPrep _missionPrep;

    public ResearchTransportCapacityPlayerAction(Research research, MissionPrep missionPrep)
    {
        _research = research;
        _missionPrep = missionPrep;
    }

    public bool CanAct() 
        => _research.CanResearchTransportCapacity();

    public void Act() 
        => _research.ResearchTransportCapacity();

    public string ActLabel()
        => $"Increase transport capacity by {_missionPrep.Data.TransportCapacity} " +
           $"for {_research.Data.TransportCapacityResearchCost}";
}