using System.Diagnostics;
using UfoGame.Model;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
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
        Staff staff,
        Timeline timeline)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
        _stateRefresh = stateRefresh;
        _missionLauncher = missionLauncher;
        _staff = staff;
        _timeline = timeline;
    }

    public void Act() => _missionLauncher.LaunchMission(_pendingMission);

    public string ActLabel()
        => $"Launch with {_staff.Data.AgentsAssignedToMissionCount} agents";

    // Range input is permanently disabled for assigning agents to mission.
    public bool CanSetRangeInput => false;

    public bool CanDecrementInput => _staff.Data.AgentsAssignedToMissionCount > 0;

    public int Input
    {
        get => _staff.Data.AgentsAssignedToMissionCount;
        // ReSharper disable once ValueParameterNotUsed
        set => Debug.Assert(false, 
            "Range input is permanently disabled for assigning agents to mission.");
    }

    public void IncrementInput()
    {
        var assignableAgents = _staff.Data
            .AssignableAgentsSortedByLaunchPriority(_timeline.CurrentTime);
        Debug.Assert(assignableAgents.Any());
        assignableAgents.First().AssignToMission();
        _stateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        var assignedAgents = _staff.Data
            .AssignedAgentsSortedByDescendingLaunchPriority(_timeline.CurrentTime);
        Debug.Assert(assignedAgents.Any());
        assignedAgents.First().UnassignFromMission();
        _stateRefresh.Trigger();
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission(_pendingMission);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission(_pendingMission, value);

    public int InputMax() => _missionPrep.MaxAgentsSendableOnMission;

    public int InputMin() => _missionPrep.MinAgentsSendableOnMission;
    
}