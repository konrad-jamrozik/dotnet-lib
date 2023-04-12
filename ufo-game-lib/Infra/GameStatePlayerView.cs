using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class GameStatePlayerView
{
    private readonly Func<GameState> _gameState;

    public GameStatePlayerView(GameSession session)
    {
        // kja this gameState lambda should really be unnecessary;
        // instead, the gameState should be mutated.
        _gameState = () => session.CurrentGameState;
    }

    public int CurrentTurn => _gameState().Timeline.CurrentTurn;
    public bool IsGameOver => _gameState().IsGameOver;
    public Missions Missions => _gameState().Missions;
    public MissionSites MissionSites => _gameState().MissionSites;
    public Assets Assets => _gameState().Assets;
}