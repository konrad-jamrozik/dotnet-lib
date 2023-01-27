using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class Timeline
{
    private readonly PlayerScore _playerScore;
    private readonly List<ITemporal> _temporals;
    private readonly Accounting _accounting;
    private readonly GameState _gameState;
    private readonly ViewStateRefresh _viewStateRefresh;

    public Timeline(
        PlayerScore playerScore,
        IEnumerable<ITemporal> temporals,
        Accounting accounting,
        GameState gameState,
        ViewStateRefresh viewStateRefresh)
    {
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
        
        if (raisedMoney)
            _accounting.AddRaisedMoney();
        
        _gameState.PersistGameState();

        _viewStateRefresh.Trigger();
    }
}