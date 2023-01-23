using System.Diagnostics;
using UfoGame.Model;
using UfoGame.Model.Data;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Agents _agents;
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;
    private readonly MissionLauncher _missionLauncher;
    private readonly Timeline _timeline;
    private readonly StateRefresh _stateRefresh;


    public LaunchMissionPlayerAction(
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        MissionLauncher missionLauncher,
        Timeline timeline,
        Agents agents)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
        _stateRefresh = stateRefresh;
        _missionLauncher = missionLauncher;
        _timeline = timeline;
        _agents = agents;
    }

    public void Act() => _missionLauncher.LaunchMission(_pendingMission);

    public string ActLabel()
        => $"Launch with {_agents.AgentsAssignedToMissionCount} agents";

    // Range input is permanently disabled for assigning agents to mission.
    public bool CanSetRangeInput => false;

    public bool CanDecrementInput => _agents.AgentsAssignedToMissionCount > 0;

    public int Input
    {
        get => _agents.AgentsAssignedToMissionCount;
        // ReSharper disable once ValueParameterNotUsed
        set => Debug.Assert(false, 
            "Range input is permanently disabled for assigning agents to mission.");
    }

    public void IncrementInput()
    {
        var assignableAgents = _agents
            .AssignableAgentsSortedByLaunchPriority(_timeline.Data.CurrentTime);
        Debug.Assert(assignableAgents.Any());
        assignableAgents.First().AssignToMission();
        _stateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        var assignedAgents = _agents
            .AssignedAgentsSortedByDescendingLaunchPriority(_timeline.Data.CurrentTime);
        Debug.Assert(assignedAgents.Any());
        assignedAgents.First().UnassignFromMission();
        _stateRefresh.Trigger();
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission(_pendingMission);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission(_pendingMission, value);

    public int InputMax() => _missionPrep.MaxAgentsSendableOnMission;

    public int InputMin() => _missionPrep.MinAgentsSendableOnMission;
    
}