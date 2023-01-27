using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Timeline
{
    public readonly TimelineData Data;
    private readonly PlayerScore _playerScore;
    private readonly List<ITemporal> _temporals;
    private readonly Accounting _accounting;
    private readonly GameState _gameState;
    private readonly ViewStateRefresh _viewStateRefresh;

    public Timeline(
        TimelineData data,
        Accounting accounting,
        GameState gameState,
        ViewStateRefresh viewStateRefresh,
        PlayerScore playerScore,
        IEnumerable<ITemporal> temporals)
    {
        Data = data;
        _accounting = accounting;
        _gameState = gameState;
        _viewStateRefresh = viewStateRefresh;
        _playerScore = playerScore;
        _temporals = temporals.ToList();
    }

    public void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!_playerScore.GameOver);
        _temporals.ForEach(temporal => temporal.AdvanceTime());
        
        // kja once the "raisedMoney" this thing is gone, Timeline can become part of Infra namespace.
        // And can be renamed, like GameStateTicker (to align with the GameState___ convention).
        if (raisedMoney)
            _accounting.AddRaisedMoney();
        
        _gameState.PersistGameState();

        _viewStateRefresh.Trigger();
    }
}