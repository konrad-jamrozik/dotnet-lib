using UfoGameLib.Infra;

namespace UfoGameLib.Tests;

public class AIPlayerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void AIPlayerPlaysGameUntilConclusion()
    {
        var aiPlayer = new AIPlayer(new GameSessionController(new GameSession()));
        aiPlayer.PlayGameSession();
    }
}