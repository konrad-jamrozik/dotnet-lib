using CommandLine;
using UfoGameLib.Infra;
using UfoGameLib.Model;

namespace UfoGameCli;

internal static class Program
{
    static void Main(string[] args)
    {
        var game = new GameSessionController(new GameSession());

        Parser.Default.ParseArguments<AdvanceTimeOptions, HireAgentsOptions, LaunchMissionOptions>(args)
            .WithParsed<AdvanceTimeOptions>(options => InvokeAdvanceTime(game))
            .WithParsed<HireAgentsOptions>(options => InvokeHireAgents(game, options.AgentCount))
            .WithParsed<LaunchMissionOptions>(
                options => InvokeLaunchMission(game, options.MissionSiteId, options.AgentCount, options.Region))
            .WithParsed<FireAgentsOptions>(options => InvokeFireAgents(game, options.AgentNames));
    }

    static void InvokeAdvanceTime(GameSessionController game)
    {
        // kja what 'game' param should this be called?
        // Note there is also potential for Game and/or GameController. Like, one needs to be able to
        // invoke a CLI command that starts a new GameSession or loads and existing one. This would be done
        // via GameController. At this stage one would also determine if to enable cheating.
        // Also, whether to use AIPlayer. See GameSessionController comment.

        game.AdvanceTime();
        Console.WriteLine("Time advanced.");
    }

    static void InvokeHireAgents(GameSessionController game, int count)
    {
        game.HireAgents(count);
        Console.WriteLine($"Hired {count} agents.");
    }

    static void InvokeLaunchMission(GameSessionController game, int siteId, int count, string region)
    {
        MissionSite site = game.GameStatePlayerView.MissionSites.Single(site => site.Id == siteId);
        game.LaunchMission(site, count);
        Console.WriteLine($"Launched mission with {count} agents in region {region}.");
    }

    static void InvokeFireAgents(GameSessionController game, IEnumerable<string> agentNames)
    {
        game.FireAgents(agentNames);
        Console.WriteLine($"Fired agents: {string.Join(", ", agentNames)}");
    }
}

// ReSharper disable ClassNeverInstantiated.Global
[Verb("advance-time", HelpText = "Advance the game time.")]
class AdvanceTimeOptions
{
}

[Verb("hire-agents", HelpText = "Hire a specific number of agents.")]
class HireAgentsOptions
{
    [Option('c', "count", Required = true, HelpText = "Number of agents to hire.")]
    public int AgentCount { get; set; }
}

[Verb("launch-mission", HelpText = "Launch a mission with a specific number of agents.")]
class LaunchMissionOptions
{
    [Option('i', "siteId", Required = true, HelpText = "ID of the mission site.")]
    public int MissionSiteId { get; set; }

    [Option('c', "count", Required = true, HelpText = "Number of agents for the mission.")]
    public int AgentCount { get; set; }

    [Option('r', "region", Required = true, HelpText = "Region for the mission.")]
    public string Region { get; set; } = "";
}

[Verb("fire-agents", HelpText = "Fire a list of agents by their names.")]
class FireAgentsOptions
{
    [Option('n', "names", Required = true, Separator = ',', HelpText = "Comma-separated list of agent names to fire.")]
    public IEnumerable<string> AgentNames { get; set; } = new List<string>();
}
// ReSharper restore ClassNeverInstantiated.Global