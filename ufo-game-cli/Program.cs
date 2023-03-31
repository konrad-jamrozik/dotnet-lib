using CommandLine;
using UfoGameLib.Tests;

namespace UfoGameCli;

internal static class Program
{
    static void Main(string[] args)
    {
        var playerSimulator = new PlayerSimulator();

        Parser.Default.ParseArguments<AdvanceTimeOptions, HireAgentsOptions, LaunchMissionOptions>(args)
            .WithParsed<AdvanceTimeOptions>(options => ExecuteAdvanceTime(playerSimulator))
            .WithParsed<HireAgentsOptions>(options => ExecuteHireAgents(playerSimulator, options.AgentCount))
            .WithParsed<LaunchMissionOptions>(options => ExecuteLaunchMission(playerSimulator, options.AgentCount));
    }

    static void ExecuteAdvanceTime(PlayerSimulator playerSimulator)
    {
        playerSimulator.AdvanceTime();
        Console.WriteLine("Time advanced.");
    }

    static void ExecuteHireAgents(PlayerSimulator playerSimulator, int count)
    {
        playerSimulator.HireAgents(count);
        Console.WriteLine($"Hired {count} agents.");
    }

    static void ExecuteLaunchMission(PlayerSimulator playerSimulator, int count)
    {
        playerSimulator.LaunchMission(count);
        Console.WriteLine($"Launched mission with {count} agents.");
    }
}

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
    [Option('c', "count", Required = true, HelpText = "Number of agents for the mission.")]
    public int AgentCount { get; set; }
}