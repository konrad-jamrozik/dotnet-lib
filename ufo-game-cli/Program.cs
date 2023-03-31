using CommandLine;
using UfoGameLib.Tests;
using System;

namespace UfoGameCli;

internal static class Program
{
    static void Main(string[] args)
    {
        var playerSimulator = new PlayerSimulator();

        Parser.Default.ParseArguments<AdvanceTimeOptions, HireAgentsOptions, LaunchMissionOptions>(args)
            .WithParsed<AdvanceTimeOptions>(options => ExecuteAdvanceTime(playerSimulator))
            .WithParsed<HireAgentsOptions>(options => ExecuteHireAgents(playerSimulator, options.AgentCount))
            .WithParsed<LaunchMissionOptions>(
                options => ExecuteLaunchMission(playerSimulator, options.AgentCount, options.Region))
            .WithParsed<FireAgentsOptions>(options => ExecuteFireAgents(playerSimulator, options.AgentNames));
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

    static void ExecuteLaunchMission(PlayerSimulator playerSimulator, int count, string region)
    {
        playerSimulator.LaunchMission(count);
        Console.WriteLine($"Launched mission with {count} agents in region {region}.");
    }

    static void ExecuteFireAgents(PlayerSimulator playerSimulator, IEnumerable<string> agentNames)
    {
        playerSimulator.FireAgents(agentNames);
        Console.WriteLine($"Fired agents: {string.Join(", ", agentNames)}");
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

    [Option('r', "region", Required = true, HelpText = "Region for the mission.")]
    public string Region { get; set; } = "";
}

[Verb("fire-agents", HelpText = "Fire a list of agents by their names.")]
class FireAgentsOptions
{
    [Option('n', "names", Required = true, Separator = ',', HelpText = "Comma-separated list of agent names to fire.")]
    public IEnumerable<string> AgentNames { get; set; } = new List<string>();
}