using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMission
{
    public int SuccessChance2 => Math.Min(100, (int)(OurPower2 / (float)(EnemyPower + OurPower2) * 100));

    public int OurPower2
    {
        get
        {
            var result = _staff.Data.SoldiersAssignedToMission
                                               .Sum(soldier => 100 + soldier.ExperienceBonus(_timeline.CurrentTime))
                                           * _staff.Data.SoldierEffectiveness
                                           / 100;
            Debug.Assert(result >= 1);
            return result;
        }
    }

    // kja got here Soldier #7 'Aleksy Sikorski' exp: 31 survived. Rolled 31 > -2147483648. Need 15.5 units of recovery.
    public int SoldierSurvivalChance2(int experienceBonus)
    {
        var adjustedSurvivabilityPower =
            (SoldierSurvivabilityPower2 * (100 + experienceBonus)) / 100;
        int result = (int)(adjustedSurvivabilityPower / (float)(EnemyPower + adjustedSurvivabilityPower) 
                           * 100);
        Debug.Assert(result >= 1);
        return result;

    }

    private int SoldierSurvivabilityPower2 => _staff.Data.SoldiersAssignedToMissionCount * _staff.Data.SoldierSurvivability;

    [JsonInclude]
    public PendingMissionData Data;

    public Faction Faction => _factions.Data.Single(f => f.Name == Data.FactionName);

    public int EnemyPower => (int)(Faction.Score * Data.EnemyPowerCoefficient);

    public int OurPower => _missionPrep.Data.SoldiersToSend * _staff.Data.SoldierEffectiveness;

    public int SuccessChance => Math.Min(100, (int)(OurPower / (float)(EnemyPower + OurPower) * 100));

    public int SoldierSurvivalChance => 
        (int)(SoldierSurvivabilityPower / (float)(EnemyPower + SoldierSurvivabilityPower) * 100);

    private int SoldierSurvivabilityPower => _missionPrep.Data.SoldiersToSend * _staff.Data.SoldierSurvivability;

    public int CountDown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;
    
    public bool MissionAboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

    // ReSharper disable once PossibleLossOfFraction
    public int MoneyReward => (int)(Faction.Score/4 * Data.MoneyRewardCoefficient);

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