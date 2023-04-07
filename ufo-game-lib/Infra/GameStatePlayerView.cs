using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class GameStatePlayerView
{
    private readonly Func<GameState> _gameState;

    public GameStatePlayerView(GameSession session)
    {
        _gameState = () => session.CurrentGameState;
    }

    public int CurrentTurn => _gameState().Timeline.CurrentTurn;
    public bool IsGameOver => _gameState().IsGameOver;
    public Missions Missions => _gameState().Missions;
    public Assets Assets => _gameState().Assets;

}