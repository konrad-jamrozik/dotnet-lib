using UfoGame.Model;

namespace UfoGame.ViewModel;

// kja wip
class LaunchMissionPlayerAction2 : IPlayerActionOnRangeInput
{
    private readonly Staff _staff;
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;
    private readonly MissionLauncher _missionLauncher;
    private readonly StateRefresh _stateRefresh;


    public LaunchMissionPlayerAction2(
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        MissionLauncher missionLauncher,
        Staff staff)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
        _stateRefresh = stateRefresh;
        _missionLauncher = missionLauncher;
        _staff = staff;
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
        return;
    }

    public void DecrementInput()
    {
        return;
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission2(_pendingMission);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission2(_pendingMission, value);

    public int InputMax() => _missionPrep.Data.TransportCapacity;

    public int InputMin() => 1;
}