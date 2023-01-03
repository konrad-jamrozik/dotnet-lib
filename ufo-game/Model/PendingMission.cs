using System.Diagnostics;

namespace UfoGame.Model;

public class PendingMission
{
    public int EnemyPower => (int)(Faction.Score * _enemyPowerCoefficient);

    public int MoneyReward;

    public int OurPower => _missionPrep.SoldiersToSend * _staff.SoldierEffectiveness;

    public int SuccessChance => Math.Min(100, (int)(OurPower / (float)(EnemyPower + OurPower) * 100));

    public int SoldierSurvivalChance => 
        (int)(SoldierSurvivabilityPower / (float)(EnemyPower + SoldierSurvivabilityPower) * 100);

    public int SoldierSurvivabilityPower => (_missionPrep.SoldiersToSend * _staff.SoldierSurvivability);

    public int AvailableIn { get; private set; }

    public int ExpiresIn { get; private set; }

    public bool CurrentlyAvailable => AvailableIn == 0 && ExpiresIn > 0;

    // kja refactor so there is no placeholder; will need to split class ctor into 
    // initial app startup and instance creation. I.e. GenerateNew() should return new instance of 
    // PendingMission instead of assigning fields.
    public Faction Faction { get; private set; } = new Faction(name: "placeholder", 0, 0);

    private float _enemyPowerCoefficient = 1;

    private readonly Random _random = new Random();
    private readonly MissionPrep _missionPrep;
    private readonly OperationsArchive _archive;
    private readonly Factions _factions;
    private readonly PlayerScore _playerScore;
    private readonly Staff _staff;

    public PendingMission(
        MissionPrep missionPrep,
        OperationsArchive archive,
        Factions factions,
        PlayerScore playerScore,
        Staff staff)
    {
        _missionPrep = missionPrep;
        _archive = archive;
        _factions = factions;
        _playerScore = playerScore;
        _staff = staff;
        GenerateNewMission();
    }

    public bool MissionAboutToExpire => CurrentlyAvailable && ExpiresIn == 1;

    public void AdvanceMissionTime()
    {
        Debug.Assert(!_playerScore.GameOver);
        Console.Out.WriteLine("PendingMission - AdvanceTime");
        if (CurrentlyAvailable)
        {
            Debug.Assert(ExpiresIn >= 1);
            if (MissionAboutToExpire)
            {
                _archive.RecordIgnoredMission();
                _playerScore.Value -= PlayerScore.IgnoreMissionScoreLoss;
                GenerateNewOrClearMission();
            }
            else
                ExpiresIn--;
        }
        else
        {
            Debug.Assert(AvailableIn >= 1);
            AvailableIn--;
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
        if (!_playerScore.GameOver)
            GenerateNewMission();
        else
            RemoveMission();
    }

    public bool CanLaunchMission(int? soldiersToSend = null)
    {
        soldiersToSend ??= _missionPrep.SoldiersToSend;
        return !_playerScore.GameOver
               && CurrentlyAvailable
               && soldiersToSend <= _missionPrep.MaxSoldiersToSend
               && soldiersToSend >= _missionPrep.MinSoldiersToSend;
    }

    private void GenerateNewMission()
    {
        Debug.Assert(!_playerScore.GameOver);
        AvailableIn = _random.Next(1, 6+1);
        ExpiresIn = _random.Next(1, 6+1);
        MoneyReward = _random.Next(10, 200 + 1);
        _enemyPowerCoefficient = _random.Next(5, 15 + 1) / (float)10;
        var undefeatedFactions = _factions.Data.Where(faction => !faction.Defeated).ToArray();
        Faction = undefeatedFactions[_random.Next(undefeatedFactions.Length)];
    }

    private void RemoveMission()
    {
        AvailableIn = 0;
        ExpiresIn = 0;
        _enemyPowerCoefficient = 1;
        Faction = new Faction(name: "-", score: 0, scoreTick: 0);
    }
}