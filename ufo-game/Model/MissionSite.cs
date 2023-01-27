using System.Diagnostics;
using UfoGame.Model.Data;

namespace UfoGame.Model;

// kja Need to rethink where to put mission stats, like "OurPower" or "SuccessChance".
// Probably in "Mission" (currently "MissionLauncher") 
public class MissionSite : ITemporal, IResettable
{
    private const int MaxAgentSurvivalChance = 99;

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
        var reducedGap = (100*survivabilityGap / (100 + experienceBonus));
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

    public int AgentSurvivabilityPower => _agents.AgentsAssignedToMissionCount * _staffData.AgentSurvivability;

    public MissionSiteData Data => _pendingMissionsData.Data[0];

    public FactionData FactionData => _factionsData.Data.Single(f => f.Name == Data.FactionName);

    public int EnemyPower => (int)(FactionData.Score * Data.EnemyPowerCoefficient);

    public int Countdown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;
    
    public bool MissionAboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

    // ReSharper disable once PossibleLossOfFraction
    public int MoneyReward => (int)(FactionData.Score/2 * Data.MoneyRewardCoefficient);

    // kja consolidate all Random gens into one
    private readonly Random _random = new Random();
    private readonly ArchiveData _archiveData;
    private readonly FactionsData _factionsData;
    private readonly PlayerScore _playerScore;
    private readonly StaffData _staffData;
    private readonly Agents _agents;
    private readonly PendingMissionsData _pendingMissionsData;

    public MissionSite(
        PendingMissionsData pendingMissionsData,
        ArchiveData archiveData,
        FactionsData factionsData,
        PlayerScore playerScore,
        StaffData staffData,
        Agents agents)
    {
        _pendingMissionsData = pendingMissionsData;
        _archiveData = archiveData;
        _factionsData = factionsData;
        _playerScore = playerScore;
        _staffData = staffData;
        _agents = agents;
        if (Data.IsNoMission)
            _pendingMissionsData.New(_playerScore, _random, _factionsData);
    }

    public void AdvanceTime()
    {
        Debug.Assert(!_playerScore.GameOver);
        if (CurrentlyAvailable)
        {
            Debug.Assert(Data.ExpiresIn >= 1);
            if (MissionAboutToExpire)
            {
                _archiveData.RecordIgnoredMission();
                _playerScore.Data.Value -= PlayerScore.IgnoreMissionScoreLoss;
                GenerateNewOrClearMission();
            }
            else
                Data.ExpiresIn--;
        }
        else
        {
            Debug.Assert(Data.AvailableIn >= 1);
            Data.AvailableIn--;
            if (CurrentlyAvailable)
            {
                if (!FactionData.Discovered)
                {
                    Console.Out.WriteLine("Discovered faction! " + FactionData.Name);
                    FactionData.Discovered = true;
                }
            }
        }
    }

    public void Reset()
    {
        _pendingMissionsData.Reset();
        GenerateNewOrClearMission();
    }

    public void GenerateNewOrClearMission()
        => _pendingMissionsData.New(_playerScore, _random, _factionsData);
}