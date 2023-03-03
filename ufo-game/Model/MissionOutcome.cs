using UfoGame.Infra;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class MissionOutcome
{
    private readonly RandomGen _randomGen;
    private readonly MissionEventLogsData _missionEventLogsData;

    public MissionOutcome(RandomGen randomGen, MissionEventLogsData missionEventLogsData)
    {
        _randomGen = randomGen;
        _missionEventLogsData = missionEventLogsData;
    }

    public (int missionRoll, bool missionSuccessful, List<AgentOutcome> agentOutcomes) Roll(
        MissionStats missionStats,
        List<Agent> sentAgents)
    {
        (int missionRoll, bool missionSuccessful) = RollMissionOutcome(missionStats);
        List<AgentOutcome> agentOutcomes = RollAgentOutcomes(missionStats, sentAgents);

        return (missionRoll, missionSuccessful, agentOutcomes);
    }

    private (int missionRoll, bool missionSuccessful) RollMissionOutcome(MissionStats missionStats)
    {
        // Roll between 1 and 100.
        // The lower the better.
        int missionRoll = _randomGen.Random.Next(1, 100 + 1);
        bool missionSuccessful = missionRoll <= missionStats.SuccessChance;
        _missionEventLogsData.Add(
            $"Rolled {missionRoll} against limit of {missionStats.SuccessChance} " +
            $"resulting in {(missionSuccessful ? "success" : "failure")}");
        return (missionRoll, missionSuccessful);
    }

    private List<AgentOutcome> RollAgentOutcomes(
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
            var agentOutcome = new AgentOutcome(agent, agentRoll, agentSurvivalChance, expBonus);
            agentOutcomes.Add(agentOutcome);
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