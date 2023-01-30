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

        // MissionSite needs to have its time advanced first as it relies on the value
        // of PlayerScore.GameOver to be not affected by time advancement; otherwise
        // precondition will fail and even if there would be no precondition, an empty mission would
        // be generated due to game being over.
        // One way the PlayerScore.GameOver could become intermittently true is if 
        // Accounting.AdvanceTime() would put player balance in the red, but then
        // _accounting.AddRaisedMoney() would keep the player still afloat, thus
        // preventing the game from being over.
        _temporals.Single(temporal => temporal.GetType() == typeof(MissionSite)).AdvanceTime();
        _temporals
            .Where(temporal => temporal.GetType() != typeof(MissionSite))
            .ToList()
            .ForEach(temporal => temporal.AdvanceTime());
        
        if (raisedMoney)
            _accounting.AddRaisedMoney();
        
        _gameState.Persist();

        _viewStateRefresh.Trigger();
    }
}