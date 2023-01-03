using UfoGame.Model;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly MissionPrep _missionPrep;
    private readonly PendingMission _pendingMission;

    public LaunchMissionPlayerAction(MissionPrep missionPrep, PendingMission pendingMission)
    {
        _missionPrep = missionPrep;
        _pendingMission = pendingMission;
    }

    public void Act()
        => _pendingMission.LaunchMission();

    public string ActLabel()
        => $"Launch with {_missionPrep.SoldiersToSend} soldiers";

    public int Input
    {
        get => _missionPrep.SoldiersToSend;
        set => _missionPrep.SoldiersToSend = value;
    }

    public void IncrementInput()
        => _missionPrep.IncrementSoldiersToSend();

    public void DecrementInput()
        => _missionPrep.DecrementSoldiersToSend();

    public bool CanAct()
        => _pendingMission.CanLaunchMission();

    public bool CanAct(int value)
        => _pendingMission.CanLaunchMission(value);

    public int InputMax()
        => _missionPrep.MaxSoldiersToSend;

    public int InputMin()
        => _missionPrep.MinSoldiersToSend;
}