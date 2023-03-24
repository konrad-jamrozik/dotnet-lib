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
    public void ConductBasicGameSession()
    {
        // kja 1 current work / TDD
        // make the test more concise:
        // - introduce assertion abstractions, like "Assert game state property P change from X to Y
        // - express the test as a series of player actions and assertions on properties, hiding the details of 
        // invoking ComputeNextGameState and the like

        var gameEngine = new GameEngine();
        GameState gameState1 = gameEngine.NewInitialGameState();

        Assert.That(gameState1.Timeline.CurrentTurn, Is.EqualTo(0));
        Assert.That(gameState1.Archive.AgentsHiredCount, Is.EqualTo(0));
        Assert.That(gameState1.Archive.MissionsLaunchedCount, Is.EqualTo(0));

        PlayerActions playerActions1 = new PlayerActions(new HireAgentsPlayerAction(count: 3));
        (GameState gameState2, GameStateComputationLog log2) = gameEngine.ComputeNextGameState(gameState1, playerActions1);

        Assert.That(gameState1.Timeline.CurrentTurn, Is.EqualTo(0));
        Assert.That(gameState2.Timeline.CurrentTurn, Is.EqualTo(1));
        Assert.That(gameState1.Archive.AgentsHiredCount, Is.EqualTo(0));
        Assert.That(gameState2.Archive.AgentsHiredCount, Is.EqualTo(3));
        Assert.That(gameState2.Archive.MissionsLaunchedCount, Is.EqualTo(0));

        PlayerActions playerActions2 = new PlayerActions(new LaunchMissionPlayerAction(agentsCount: 3));
        (GameState gameState3, GameStateComputationLog log3) = gameEngine.ComputeNextGameState(gameState2, playerActions2);

        Assert.That(gameState2.Timeline.CurrentTurn, Is.EqualTo(1));
        Assert.That(gameState3.Timeline.CurrentTurn, Is.EqualTo(2));
        Assert.That(gameState3.Archive.AgentsHiredCount, Is.EqualTo(3));
        Assert.That(gameState2.Archive.MissionsLaunchedCount, Is.EqualTo(0));
        Assert.That(gameState3.Archive.MissionsLaunchedCount, Is.EqualTo(1));
    }
}