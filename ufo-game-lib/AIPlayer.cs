using UfoGameLib.Infra;
using UfoGameLib.Model;

namespace UfoGameLib;

public class AIPlayer
{
    private const int MaxAgentsToSendOnMission = 2;
    private readonly GameSessionController _game;

    public AIPlayer(GameSessionController game)
    {
        _game = game;
    }

    public void PlayGameSession()
    {
        GameStatePlayerView state = _game.GameStatePlayerView;
        while (!state.IsGameOver)
        {
            Console.Out.WriteLine(
                $"----- AIPlayer Current turn: {state.CurrentTurn} Current money: {state.Assets.CurrentMoney}");
            while (state.MissionSites.Any(site => site.IsActive) && state.Assets.CurrentTransportCapacity > 0)
            {
                MissionSite targetSite = state.MissionSites.First(site => site.IsActive);
                int agentsToHire = Math.Max(
                    Math.Min(state.Assets.CurrentTransportCapacity, MaxAgentsToSendOnMission)
                    - state.Assets.Agents.Count,
                    0);
                _game.HireAgents(agentsToHire);
                _game.LaunchMission(targetSite, state.Assets.Agents.Count);
            }
            Console.Out.WriteLine(
                $"----- AIPlayer Current turn: {state.CurrentTurn} DONE");
            // kja this _game.AdvanceTime(), the while !GameOver and reference to state should come from base abstract/template method.
            _game.AdvanceTime();
        }
        // kja to implement AI Player
        // DONE First level:
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