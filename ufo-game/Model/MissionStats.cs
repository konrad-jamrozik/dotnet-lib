using System.Diagnostics;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class MissionStats
{
    private const int MaxAgentSurvivalChance = 99;
    private readonly MissionSitesData _missionSitesData;
    private readonly FactionsData _factionsData;
    private readonly StaffData _staffData;
    private readonly Agents _agents;

    public MissionStats(
        MissionSitesData missionSitesData,
        FactionsData factionsData,
        StaffData staffData,
        Agents agents)
    {
        _missionSitesData = missionSitesData;
        _factionsData = factionsData;
        _staffData = staffData;
        _agents = agents;
    }

    public int SuccessChance => Math.Min(100, (int)(OurPower / (float)(EnemyPower + OurPower) * 100));

    public int OurPower
    {
        get
        {
            var result
                = _agents.AgentsAssignedToMission
                      .Sum(agent => 100 + agent.ExperienceBonus())
                  * _staffData.AgentEffectiveness
                  / 100;
            Debug.Assert(result >= 0);
            return result;
        }
    }

    public int EnemyPower => (int)(FactionData.Score * MissionSiteData.EnemyPowerCoefficient);

    // ReSharper disable once PossibleLossOfFraction
    public int MoneyReward => (int)(FactionData.Score / 2 * MissionSiteData.MoneyRewardCoefficient);

    public int AgentSurvivalChance(int experienceBonus)
    {
        // Agent experience bonus divides the remaining gap in survivability, to 99%.
        // For example, if baseline survivability is 30%, the gap to 99% is 99%-30%=69%.

        // If a agent has 200% experience bonus, the gap is shrunk
        // from 69% to 69%/(1+200%) = 69%*(1/3) = 23%. So survivability goes up from 99%-69%=30% to 99%-23%=76%.
        //
        // If a agent has 50% experience bonus, the gap is shrunk
        // from 69% to 69%/(1+50%) = 69%*(2/3) = 46%. So survivability goes up from 99%-69%=30% to 99%-46%=53%.
        // 
        var survivabilityGap = MaxAgentSurvivalChance - BaselineAgentSurvivalChance;
        Debug.Assert(survivabilityGap is >= 0 and <= MaxAgentSurvivalChance,
            $"survivabilityGap {survivabilityGap} is >= 0 " +
            $"and <= MaxAgentSurvivalChance {MaxAgentSurvivalChance}. " +
            $"BaselineAgentSurvivalChance: {BaselineAgentSurvivalChance}");
        var reducedGap = (100 * survivabilityGap / (100 + experienceBonus));
        var newSurvivalChance = MaxAgentSurvivalChance - reducedGap;
        Debug.Assert(newSurvivalChance >= BaselineAgentSurvivalChance, "newSurvivalChance >= BaselineAgentSurvivalChance");
        Debug.Assert(newSurvivalChance <= MaxAgentSurvivalChance, "newSurvivalChance <= MaxAgentSurvivalChance");
        return newSurvivalChance;
    }

    public int BaselineAgentSurvivalChance
    {
        get
        {
            var baselineAgentSurvivalChance =
                (int)(AgentSurvivabilityPower / (float)(EnemyPower + AgentSurvivabilityPower) * 100);
            return Math.Min(baselineAgentSurvivalChance, MaxAgentSurvivalChance);
        }
    }

    private int AgentSurvivabilityPower => _agents.AgentsAssignedToMissionCount * _staffData.AgentSurvivability;

    private MissionSiteData MissionSiteData => _missionSitesData.Data[0];

    private FactionData FactionData => _factionsData.Data.Single(f => f.Name == MissionSiteData.FactionName);
}