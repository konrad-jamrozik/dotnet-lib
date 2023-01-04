using UfoGame.Model;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;
    private readonly StateRefresh _stateRefresh;

    public LaunchMissionPlayerAction(
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
        _stateRefresh = stateRefresh;
    }

    public void Act() => _pendingMission.LaunchMission();

    public string ActLabel()
        => $"Launch with {_missionPrep.SoldiersToSend} soldiers";

    public int Input
    {
        get => _missionPrep.SoldiersToSend;
        set
        {
            _missionPrep.SoldiersToSend = value; 
            _stateRefresh.Trigger();
        }
    }

    public void IncrementInput()
    {
        _missionPrep.SoldiersToSend += 1;
        _stateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        _missionPrep.SoldiersToSend -= 1;
        _stateRefresh.Trigger();
    }

    public bool CanAct() => _pendingMission.CanLaunchMission();

    public bool CanAct(int value) => _pendingMission.CanLaunchMission(value);

    public int InputMax() => _missionPrep.MaxSoldiersToSend;

    public int InputMin() => _missionPrep.MinSoldiersToSend;
}