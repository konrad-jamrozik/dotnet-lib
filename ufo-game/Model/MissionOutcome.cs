﻿using UfoGame.Infra;

namespace UfoGame.Model;

public class MissionOutcome
{
    // kja consolidate all Random gens into one: RandomGen
    private readonly RandomGen _randomGen;

    public MissionOutcome(RandomGen randomGen)
    {
        _randomGen = randomGen;
    }

    public (int missionRoll, bool missionSuccessful, List<AgentOutcome> agentOutcomes) Roll(
        MissionStats missionStats,
        List<Agent> sentAgents)
    {
        (int missionRoll, bool missionSuccessful) = RollMissionOutcome(missionStats);
        List<AgentOutcome> agentOutcomes = RollAgentOutcomes(missionStats, sentAgents);
        return (missionRoll, missionSuccessful, agentOutcomes);
    }

    // kja introduce class like "MissionOutcome" which will have method like "roll" and
    // will leverage MissionStats (to be introduced).
    // That class will produce everything that needs to change as a result of the mission
    // Then the launcher can do missionOutcome.Apply(), thus updating agents state,
    // archiving things, giving mission rewards, etc.
    public (int missionRoll, bool missionSuccessful) RollMissionOutcome(MissionStats missionStats)
    {
        // Roll between 1 and 100.
        // The lower the better.
        int missionRoll = _randomGen.Random.Next(1, 100 + 1);
        bool missionSuccessful = missionRoll <= missionStats.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {missionRoll} against limit of {missionStats.SuccessChance} " +
            $"resulting in {(missionSuccessful ? "success" : "failure")}");
        return (missionRoll, missionSuccessful);
    }

    public List<AgentOutcome> RollAgentOutcomes(
        MissionStats missionStats,
        List<Agent> sentAgents)
    {
        var agentOutcomes = new List<AgentOutcome>();

        foreach (Agent agent in sentAgents)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int agentRoll = _randomGen.Random.Next(1, 100 + 1);
            var expBonus = agent.ExperienceBonus();
            var agentSurvivalChance
                = missionStats.AgentSurvivalChance(expBonus);
            agentOutcomes.Add(new AgentOutcome(agent, agentRoll, agentSurvivalChance, expBonus));
        }

        return agentOutcomes;
    }

    public record AgentOutcome(Agent Agent, int Roll, int SurvivalChance, int ExpBonus)
    {
        public bool Survived => Roll <= SurvivalChance;
        public bool Lost => !Survived;

        /// <summary>
        /// Higher roll means it was a closer call, so agent needs more time to recover from fatigue 
        /// and wounds. This means that if a agent is very good at surviving, they may barely survive,
        /// but need tons of time to recover.
        /// </summary>
        public float Recovery(bool missionSuccessful) => (float)Math.Round(Roll * (missionSuccessful ? 0.5f : 1), 2);
    }
}