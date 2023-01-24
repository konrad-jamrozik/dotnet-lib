using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Timeline
{
    public readonly TimelineData Data;
    private readonly FactionsData _factionsData;
    private readonly PendingMission _pendingMission;
    private readonly Accounting _accounting;
    private readonly GameState _gameState;
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly PlayerScore _playerScore;
    private readonly SickBay _sickBay;

    public Timeline(
        TimelineData data,
        FactionsData factionsData,
        PendingMission pendingMission,
        Accounting accounting,
        GameState gameState,
        ViewStateRefresh viewStateRefresh,
        PlayerScore playerScore,
        SickBay sickBay)
    {
        Data = data;
        _factionsData = factionsData;
        _pendingMission = pendingMission;
        _accounting = accounting;
        _gameState = gameState;
        _viewStateRefresh = viewStateRefresh;
        _playerScore = playerScore;
        _sickBay = sickBay;
    }

    public void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!_playerScore.GameOver);
        
        // kja instead use reflection to call AdvanceTime() on a collection
        // of all injected types that implement ITemporal.
        // Note: need to be careful about ordering. Ideally it shouldn't matter.
        // For example, _pendingMission.AdvanceTime(); may generate new mission,
        // which calculates values based on faction. It shouldn't matter
        // if faction time was advanced before or after the mission got generated.
        Data.AdvanceTime();
        _pendingMission.AdvanceTime();
        _factionsData.AdvanceTime();
        _accounting.AdvanceTime();
        _sickBay.AdvanceTime();
        
        if (raisedMoney)
            _accounting.AddRaisedMoney();
        
        _gameState.PersistGameState();

        _viewStateRefresh.Trigger();
    }
}