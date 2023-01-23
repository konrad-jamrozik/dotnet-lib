using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Timeline
{
    public readonly TimelineData Data;
    private readonly Factions _factions;
    private readonly PendingMission _pendingMission;
    private readonly Accounting _accounting;
    private readonly GameState _gameState;
    private readonly StateRefresh _stateRefresh;
    private readonly PlayerScore _playerScore;
    private readonly SickBay _sickBay;

    public Timeline(
        TimelineData data,
        Factions factions,
        PendingMission pendingMission,
        Accounting accounting,
        GameState gameState,
        StateRefresh stateRefresh,
        PlayerScore playerScore,
        SickBay sickBay)
    {
        Data = data;
        _factions = factions;
        _pendingMission = pendingMission;
        _accounting = accounting;
        _gameState = gameState;
        _stateRefresh = stateRefresh;
        _playerScore = playerScore;
        _sickBay = sickBay;
    }

    public void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!_playerScore.GameOver);
        Data.AdvanceTime();
        _pendingMission.AdvanceTime();
        _factions.AdvanceTime();
        _accounting.AdvanceTime(raisedMoney);
        _sickBay.AdvanceTime();
        _gameState.PersistGameState();
        _stateRefresh.Trigger();
    }
}