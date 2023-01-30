using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Timeline
{
    private readonly Accounting _accounting;
    private readonly PlayerScore _playerScore;
    private readonly List<ITemporal> _temporals;
    private readonly GameState _gameState;
    private readonly ViewStateRefresh _viewStateRefresh;

    public Timeline(
        Accounting accounting,
        PlayerScore playerScore,
        IEnumerable<ITemporal> temporals,
        GameState gameState,
        ViewStateRefresh viewStateRefresh)
    {
        _accounting = accounting;
        _playerScore = playerScore;
        _temporals = temporals.ToList();
        _gameState = gameState;
        _viewStateRefresh = viewStateRefresh;
    }

    public void AdvanceTime(bool raisedMoney = false)
    {
        Debug.Assert(!_playerScore.GameOver);
        _temporals.ForEach(temporal => temporal.AdvanceTime());
        
        if (raisedMoney)
            _accounting.AddRaisedMoney();
        
        _gameState.Persist();

        _viewStateRefresh.Trigger();
    }
}