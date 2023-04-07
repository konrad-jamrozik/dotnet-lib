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
        GameStatePlayerView state = _game.GameStatePlayerView;
        while (!state.IsGameOver)
        {
            Console.Out.WriteLine(
                $"AIPlayer.PlayGame Current turn: {state.CurrentTurn} Current money: {state.Assets.CurrentMoney}");
            List<Mission> availableMissions = state.Missions;
            while (availableMissions.Any() && state.Assets.TransportCapacity > 0)
            {
                Mission targetMission = availableMissions.First();
                int missingAgents = Math.Max(state.Assets.TransportCapacity - state.Assets.Agents.Count, 0);
                _game.HireAgents(missingAgents);
                _game.LaunchMission(targetMission, state.Assets.Agents.Count);
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