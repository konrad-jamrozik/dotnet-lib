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

    [Test]
    public void ConductBasicHappyPathGameSession()
    {
        PlayerSimulator sim = new PlayerSimulator();

        var startingGameState = sim.GameSession.CurrentGameState;

        Assert.Multiple(
            () =>
            {
                Assert.That(startingGameState.Timeline.CurrentTurn, Is.EqualTo(0));
                Assert.That(startingGameState.Archive.AgentsHiredCount, Is.EqualTo(0));
                Assert.That(startingGameState.Archive.MissionsLaunchedCount, Is.EqualTo(0));
            });

        // Act
        sim.HireAgents(count: 3);
        sim.LaunchMission(agentCount: 3);

        var finalGameState = sim.GameSession.CurrentGameState;

        Assert.Multiple(() => {
            Assert.That(finalGameState.Timeline.CurrentTurn, Is.EqualTo(2));
            Assert.That(finalGameState.Archive.AgentsHiredCount, Is.EqualTo(3));
            Assert.That(finalGameState.Archive.MissionsLaunchedCount, Is.EqualTo(1));

            Assert.That(startingGameState, Is.EqualTo(sim.GameSession.GameStates.First()));
        });
    }
}