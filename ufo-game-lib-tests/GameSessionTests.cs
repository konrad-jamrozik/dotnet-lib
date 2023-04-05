namespace UfoGameLib.Tests;

public class GameSessionTests
{
    [SetUp]
    public void Setup()
    {
    }

    // kja overall work plan:
    // reimplement a bit more logic
    //   do so by starting with enumerating the to-implement capabilities of AutomatedPlayer.
    //   Inheritance hierarchy:
    //     HumanPlayer     : Player
    //     AutomatedPlayer : Player
    //   A Player can invoke GameSessionController methods to exert actions upon the game world.
    //   Player can also invoke methods on GameSessionController to query player-visible parts of GameState.
    //
    //   AutomatedPlayer has a method like .PlayFullGameSession(GameSessionController) that allows it to play
    //   the game session until the end, using its internal logic.
    //
    //   HumanPlayer doesn't have it, as it doesn't make any sense - HumanPlayer class reads input from
    //   an actual human, fwds them to the GameSessionController, and returns the output back to the human.
    //
    //   Actually, having HumanPlayer seems dumb - see comments on ufo-game-cli.
    //   So maybe the actual idea is like that, with following scenarios possible:
    //                            AutomatedPlayer <--> GameSessionController <--> GameSession
    //               Human <--.exe--> CLI Program <--> GameSessionController <--> GameSession
    //   automated process <--.exe--> CLI Program <--> GameSessionController <--> GameSession
    //   special case: use CLI program to start a new game session and launch AutomatedPlayer
    //     to play it.
    //
    //   Here the GameSessionController provides convenience methods and by default restricts
    //   access, unless admin-mode methods are invoked. They may require passing as argument
    //   some form of "capability" (~permission), or the entire Controller needs to be instantiated
    //   in admin mode, e.g. via inheritance, e.g. GameSessionAdmin(or Debug)Controller
    //   E.g. something like: the CLI can access full GameState via GameSessionController or do "invalid" operations
    //   (like player conjuring lots of money) but for that it needs to pass "-cheat" (or "-debug") flag.
    //   And the GameSessionController "cheaty" method implementations do precondition check
    //   if the "-cheat" flag was passed. This would prevent human or process using CLI to avoid using the flag,
    //   but AutomatedPlayer could still do it. Which is fine I guess, no need to restrict it.
    //   Alternatively, maybe there should be a separate class, like CheatingGameSessionController, that
    //   maybe just inherits from GameSessionController to provide full access to GameSession state + cheat methods.
    //   Then AutomatedPlayer would only use GameSessionController and any implementers of it thus would only have
    //   access to it, not CheatingGameSessionController.
    //
    //   Maybe I need something like Abstract Factory pattern? Where GameSession has GameState,
    //   but a derived GameSession, like GameSessionForPlayer : GameSession,
    //   narrows down the GameState property to be PlayerVisibleGameState : GameState.
    //   https://www.dofactory.com/net/abstract-factory-design-pattern
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
        GameSessionController controller = new GameSessionController(session);

        var startingGameState = session.CurrentGameState;

        Assert.Multiple(
            () =>
            {
                Assert.That(startingGameState.Timeline.CurrentTurn, Is.EqualTo(0));
                Assert.That(startingGameState.Assets.Agents.Count, Is.EqualTo(0));
                Assert.That(startingGameState.Missions.Count, Is.EqualTo(0));
            });

        // Act
        controller.HireAgents(count: 3);
        controller.AdvanceTime();
        controller.LaunchMission(agentCount: 3);
        controller.AdvanceTime();

        var finalGameState = session.CurrentGameState;

        Assert.Multiple(() => {
            Assert.That(finalGameState.Timeline.CurrentTurn, Is.EqualTo(2), "currentTurn");
            Assert.That(finalGameState.Assets.Agents.Count, Is.EqualTo(3), "agentsHiredCount");
            Assert.That(finalGameState.Missions.Count, Is.EqualTo(1), "missionsLaunchedCount");

            Assert.That(startingGameState, Is.EqualTo(session.GameStates.First()), "states should be equal");
            Assert.That(startingGameState.Assets.Agents, Is.Not.EqualTo(finalGameState.Assets.Agents));
            Assert.That(startingGameState.Missions, Is.Not.EqualTo(finalGameState.Missions));
        });
    }
}