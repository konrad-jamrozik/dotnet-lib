using UfoGameLib.Infra;
using UfoGameLib.Model;

namespace UfoGameLib;

public class AIPlayer
{
    private readonly GameSessionController _game;

    public AIPlayer(GameSessionController game)
    {
        _game = game;
    }

    public void PlayGame()
    {
        while (!_game.IsGameOver)
        {
            List<Mission> availableMissions = _game.AvailableMissions;
            while (availableMissions.Any() && _game.TransportCapacity > 0)
            {
                var targetMission = availableMissions.First();
                var missingAgents = Math.Max(_game.TransportCapacity - _game.Agents.Count, 0);
                _game.HireAgents(missingAgents);
                _game.LaunchMission(targetMission, _game.Agents.Count);
            }
            _game.AdvanceTime();
        }
        // kja to implement AI Player
        // First level:
        // - Advance time until mission available
        // - Once mission available, hire agents up to transport limit and send on mission
        // - Repeat until player loses game (at this point impossible to win)
        // 
        // Next level:
        // - Try to always keep at least enough agents to maintain full transport capacity
        // - Send agents on intel-gathering duty until mission is available
        // - Send agents on available mission if not too hard
        // - Do not hire agents if it would lead to bankruptcy
    }
}