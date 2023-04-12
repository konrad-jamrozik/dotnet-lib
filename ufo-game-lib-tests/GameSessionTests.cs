using UfoGameLib.Infra;
using UfoGameLib.Model;

namespace UfoGameLib.Tests;

public class GameSessionTests
{
    [SetUp]
    public void Setup()
    {
    }

    // kja overall work plan:
    // implement first AIPlayer
    //
    // reimplement a bit more logic
    //   start by adding new capabilities to AIPlayer (as if it tried to do a new thing) and implement going from there.
    //
    // implement IPersistable
    //   write unit tests confirming it works
    //
    // implement IResettable
    //   write unit tests confirming it works
    //
    // implement ITemporal
    //   write unit tests confirming it works
    //
    // generate CLI interface based on available PlayerActions and contents of GameState
    //
    // generate API Controller methods for REST API generation
    //
    // deploy the game server with REST API locally
    //
    // deploy the game server with REST API to Azure
    //
    // generate swagger UI from the controller methods
    //
    // when available, interface with the swagger UI via LLM, or with CLI by using GH CLI Copilot
    //
    // Test strategy:
    // - One basic happy path test, showcasing concrete steps how player could interact with the API,
    // via usage of player simulator.
    // - Smart player simulators, actually playing the game, designed in a way to exercise its features.
    //   - Such simulators will exercise all of the game logic by design, and I could add assertions checking
    //     if given feature was used at least once during the simulated run.
    //   - Game sessions executed by this players will be captured as unit tests, by fixing appropriate
    //     random seed and letting the simulator play.
    // - All code augmented with strong suite of invariants: preconditions, postconditions, assertions.
    //   - This, coupled with the smart player simulations, ensures test failure on invariant violation.

    [Test]
    public void ConductBasicHappyPathGameSession()
    {
        var session = new GameSession();
        var game = new GameSessionController(session);

        var startingGameState = session.CurrentGameState;

        Assert.Multiple(
            () =>
            {
                Assert.That(startingGameState.Timeline.CurrentTurn, Is.EqualTo(1));
                Assert.That(startingGameState.Assets.Agents, Has.Count.EqualTo(0));
                Assert.That(startingGameState.Missions, Has.Count.EqualTo(0));
            });

        // Act
        game.HireAgents(count: 3);
        game.AdvanceTime();
        game.AdvanceTime();
        MissionSite site = game.GameStatePlayerView.MissionSites.First();
        game.LaunchMission(site, agentCount: 3);
        game.AdvanceTime();

        var finalGameState = session.CurrentGameState;

        Assert.Multiple(() => {
            Assert.That(finalGameState.Timeline.CurrentTurn, Is.EqualTo(4), "currentTurn");
            Assert.That(finalGameState.Assets.Agents, Has.Count.EqualTo(3), "agentsHiredCount");
            Assert.That(finalGameState.Missions, Has.Count.EqualTo(1), "missionsLaunchedCount");

            Assert.That(startingGameState, Is.EqualTo(session.GameStates.First()), "states should be equal");
            Assert.That(startingGameState.Assets.Agents, Is.Not.EqualTo(finalGameState.Assets.Agents));
            Assert.That(startingGameState.Missions, Is.Not.EqualTo(finalGameState.Missions));
        });
    }

    // kja curr TDD test
    [Test]
    public void SaveAndLoadGameSession()
    {
        var session = new GameSession();
        var game = new GameSessionController(session);

        var startingGameState = session.CurrentGameState;

        var savedTurn = game.GameStatePlayerView.CurrentTurn;
        game.Save();
        game.AdvanceTime();
        Assert.That(game.GameStatePlayerView.CurrentTurn, Is.EqualTo(savedTurn + 1), "savedTurn+1");
        game.Load();
        Assert.That(game.GameStatePlayerView.CurrentTurn, Is.EqualTo(savedTurn), "savedTurn");
    }

}