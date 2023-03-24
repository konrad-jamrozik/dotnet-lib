namespace UfoGameLib.Tests;

public class GameSessionTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ConductBasicGameSession()
    {
        // kja current work / TDD

        var gameEngine = new GameEngine();
        GameState gameState1 = gameEngine.NewInitialGameState();

        Assert.That(gameState1.Archive.AgentsHiredCount, Is.EqualTo(0));
        Assert.That(gameState1.Archive.MissionsLaunchedCount, Is.EqualTo(0));

        PlayerActions playerActions1 = new PlayerActions(new HireAgentsPlayerAction(count: 3));
        (GameState gameState2, GameStateComputationLog log2) = gameEngine.ComputeNextGameState(gameState1, playerActions1);

        Assert.That(gameState1.Archive.AgentsHiredCount, Is.EqualTo(0));
        Assert.That(gameState2.Archive.AgentsHiredCount, Is.EqualTo(3));
        Assert.That(gameState2.Archive.MissionsLaunchedCount, Is.EqualTo(0));

        PlayerActions playerActions2 = new PlayerActions(new LaunchMissionPlayerAction(agentsCount: 3));
        (GameState gameState3, GameStateComputationLog log3) = gameEngine.ComputeNextGameState(gameState2, playerActions2);

        Assert.That(gameState3.Archive.AgentsHiredCount, Is.EqualTo(3));
        Assert.That(gameState2.Archive.MissionsLaunchedCount, Is.EqualTo(0));
        Assert.That(gameState3.Archive.MissionsLaunchedCount, Is.EqualTo(1));
    }
}