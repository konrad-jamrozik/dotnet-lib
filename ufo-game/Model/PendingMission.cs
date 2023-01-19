using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMission
{
    private const int MaxSurvivalChance = 99;

    public int SuccessChance => Math.Min(100, (int)(OurPower / (float)(EnemyPower + OurPower) * 100));

    public int OurPower
    {
        get
        {
            var result
                = _staff.Data.SoldiersAssignedToMission
                      .Sum(soldier => 100 + soldier.ExperienceBonus(_timeline.CurrentTime))
                  * _staff.Data.SoldierEffectiveness
                  / 100;
            Debug.Assert(result >= 0);
            return result;
        }
    }

    public int SoldierSurvivalChance(int experienceBonus)
    {
        // Soldier experience bonus divides the remaining gap in survivability, to 99%.
        // For example, if baseline survivability is 30%, the gap to 99% is 99%-30%=69%.

        // If a soldier has 200% experience bonus, the gap is shrunk
        // from 69% to 69%/(1+200%) = 69%*(1/3) = 23%. So survivability goes up from 99%-69%=30% to 99%-23%=76%.
        //
        // If a soldier has 50% experience bonus, the gap is shrunk
        // from 69% to 69%/(1+50%) = 69%*(2/3) = 46%. So survivability goes up from 99%-69%=30% to 99%-46%=53%.
        // 
        var survivabilityGap = MaxSurvivalChance - BaselineSoldierSurvivalChance;
        Debug.Assert(survivabilityGap is >= 0 and <= MaxSurvivalChance, 
            $"survivabilityGap {survivabilityGap} is >= 0 " +
            $"and <= MaxSurvivalChance {MaxSurvivalChance}. " +
            $"BaselineSoldierSurvivalChance: {BaselineSoldierSurvivalChance}");
        var reducedGap = (100*survivabilityGap / (100 + experienceBonus));
        var newSurvivalChance = MaxSurvivalChance - reducedGap;
        Debug.Assert(newSurvivalChance >= BaselineSoldierSurvivalChance, "newSurvivalChance >= BaselineSoldierSurvivalChance");
        Debug.Assert(newSurvivalChance <= MaxSurvivalChance, "newSurvivalChance <= MaxSurvivalChance");
        return newSurvivalChance;
    }

    public int BaselineSoldierSurvivalChance
    {
        get
        {
            var baselineSoldierSurvivalChance =
                (int)(SoldierSurvivabilityPower / (float)(EnemyPower + SoldierSurvivabilityPower) * 100);
            return Math.Min(baselineSoldierSurvivalChance, MaxSurvivalChance);
        }
    }

    public int SoldierSurvivabilityPower => _staff.Data.SoldiersAssignedToMissionCount * _staff.Data.SoldierSurvivability;

    [JsonInclude]
    public PendingMissionData Data;

    public Faction Faction => _factions.Data.Single(f => f.Name == Data.FactionName);

    public int EnemyPower => (int)(Faction.Score * Data.EnemyPowerCoefficient);

    public int CountDown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;
    
    public bool MissionAboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

    // ReSharper disable once PossibleLossOfFraction
    public int MoneyReward => (int)(Faction.Score/2 * Data.MoneyRewardCoefficient);

    private readonly Random _random = new Random();
    private readonly MissionPrep _missionPrep;
    private readonly Archive _archive;
    private readonly Factions _factions;
    private readonly PlayerScore _playerScore;
    private readonly Staff _staff;
    private readonly Timeline _timeline;

    public PendingMission(
        PendingMissionData data,
        MissionPrep missionPrep,
        Archive archive,
        Factions factions,
        PlayerScore playerScore,
        Staff staff,
        Timeline timeline)
    {
        Data = data;
        _missionPrep = missionPrep;
        _archive = archive;
        _factions = factions;
        _playerScore = playerScore;
        _staff = staff;
        _timeline = timeline;
        if (data.IsNoMission)
            Data = PendingMissionData.New(_playerScore, _random, _factions);
    }

    public void AdvanceMissionTime()
    {
        Debug.Assert(!_playerScore.GameOver);
        Console.Out.WriteLine("PendingMission - AdvanceTime");
        if (CurrentlyAvailable)
        {
            Debug.Assert(Data.ExpiresIn >= 1);
            if (MissionAboutToExpire)
            {
                _archive.RecordIgnoredMission();
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
                if (!Faction.Discovered)
                {
                    Console.Out.WriteLine("Discovered faction! " + Faction.Name);
                    Faction.Discovered = true;
                }
            }
        }
    }

    public void GenerateNewOrClearMission()
    {
        Data = PendingMissionData.New(_playerScore, _random, _factions);
    }

    public void Reset()
    {
        GenerateNewOrClearMission();
    }
}