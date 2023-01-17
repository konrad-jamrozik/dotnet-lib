using System.Diagnostics;
using UfoGame.Model;

namespace UfoGame.ViewModel;

// kja wip
class LaunchMissionPlayerAction2 : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;
    private readonly MissionLauncher _missionLauncher;
    private readonly Timeline _timeline;
    private readonly StateRefresh _stateRefresh;


    public LaunchMissionPlayerAction2(
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

    public void Act() => _missionLauncher.LaunchMission2(_pendingMission);

    public string ActLabel()
        => $"Launch with {_staff.Data.SoldiersAssignedToMissionCount} soldiers";

    public int Input
    {
        get => _staff.Data.SoldiersAssignedToMissionCount;
        set
        {
            return;
        }
    }

    public void IncrementInput()
    {
        var assignableSoldiers = _staff.Data
            .AssignableSoldiersSortedByLaunchPriority(_timeline.CurrentTime);
        Debug.Assert(assignableSoldiers.Any());
        assignableSoldiers.First().AssignToMission();
        _stateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        var assignedSoldiers = _staff.Data
            .AssignedSoldiersSortedByDescendingLaunchPriority(_timeline.CurrentTime);
        Debug.Assert(assignedSoldiers.Any());
        assignedSoldiers.First().UnassignFromMission();
        _stateRefresh.Trigger();
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission2(_pendingMission);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission2(_pendingMission, value);

    public int InputMax() => _missionPrep.MaxSoldiersSendableOnMission;

    public int InputMin() => _missionPrep.MinSoldiersSendableOnMission;


}