using UfoGame.Model;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;
    private readonly MissionLauncher _missionLauncher;
    private readonly StateRefresh _stateRefresh;


    public LaunchMissionPlayerAction(
        MissionPrep missionPrep,
        PendingMission pendingMission,
        StateRefresh stateRefresh,
        MissionLauncher missionLauncher)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
        _stateRefresh = stateRefresh;
        _missionLauncher = missionLauncher;
    }

    public void Act() => _missionLauncher.LaunchMission(_pendingMission);

    public string ActLabel()
        => $"Launch with {_missionPrep.Data.SoldiersToSend} soldiers";

    public int Input
    {
        get => _missionPrep.Data.SoldiersToSend;
        set
        {
            _missionPrep.Data.SoldiersToSend = value; 
            _stateRefresh.Trigger();
        }
    }

    public void IncrementInput()
    {
        _missionPrep.Data.SoldiersToSend += 1;
        _stateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        _missionPrep.Data.SoldiersToSend -= 1;
        _stateRefresh.Trigger();
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission(_pendingMission);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission(_pendingMission, value);

    public int InputMax() => _missionPrep.MaxSoldiersToSend;

    public int InputMin() => _missionPrep.MinSoldiersToSend;
}