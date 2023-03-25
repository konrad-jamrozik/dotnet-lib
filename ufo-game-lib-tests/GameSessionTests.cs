namespace UfoGameLib.Tests;

public class GameSessionTests
{
    [SetUp]
    public void Setup()
    {
    }

    // kja 2 upcoming work:
    // reimplement a bit more logic
    // implement IPersistable
    //   write unit tests confirming it works
    // implement IResettable
    //   write unit tests confirming it works
    // implement ITemporal
    //   write unit tests confirming it works
    // generate CLI interface based on available PlayerActions and contents of GameState
    // generate API Controller methods for REST API generation
    // deploy the game server with REST API locally
    // deploy the game server with REST API to Azure
    // generate swagger UI from the controller methods
    // when available, interface with the swagger UI via LLM, or with CLI by using GH CLI Copilot

    // Test strategy:
    // - One basic happy path test, showcasing concrete steps how player could interact with the API,
    // via usage of player simulator.
    // - Smart player simulators, actually playing the game, designed in a way to exercise its features.
    //   - Such simulators will exercise all of game logic by design, and some assertions may get made if given
    //     feature was used at all.
    //   - Game sessions executed by this players will be captured as unit tests, by fixing appropriate
    //     random seed and letting the simulator play.
    // - All code augmented with strong suite of invariants: preconditions, postconditions, assertions.
    //   - This, coupled with the smart player simulations, ensures test failure on invariant violation.

    [Test]
    public void ConductBasicHappyPathGameSession()
    {
        PlayerSimulator sim = new PlayerSimulator();

        var startingGameState = sim.GameSession.CurrentGameState;

        Assert.Multiple(
            () =>
            {
                Assert.That(startingGameState.Timeline.CurrentTurn, Is.EqualTo(0));
                Assert.That(startingGameState.Assets.Agents.Count, Is.EqualTo(0));
                Assert.That(startingGameState.Missions.Count, Is.EqualTo(0));
            });

        // Act
        sim.HireAgents(count: 3);
        sim.AdvanceTime();
        sim.LaunchMission(agentCount: 3);
        sim.AdvanceTime();

        var finalGameState = sim.GameSession.CurrentGameState;

        Assert.Multiple(() => {
            Assert.That(finalGameState.Timeline.CurrentTurn, Is.EqualTo(2), "currentTurn");
            Assert.That(finalGameState.Assets.Agents.Count, Is.EqualTo(3), "agentsHiredCount");
            Assert.That(finalGameState.Missions.Count, Is.EqualTo(1), "missionsLaunchedCount");

            Assert.That(startingGameState, Is.EqualTo(sim.GameSession.GameStates.First()), "states are different");
            Assert.That(startingGameState.Assets.Agents, Is.Not.EqualTo(finalGameState.Assets.Agents));
            Assert.That(startingGameState.Missions, Is.Not.EqualTo(finalGameState.Missions));
        });
    }
}