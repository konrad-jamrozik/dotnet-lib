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
    private readonly Money _money;
    private readonly StateRefresh _stateRefresh;

    public PendingMission(
        MissionPrep missionPrep,
        OperationsArchive archive,
        Factions factions,
        PlayerScore playerScore,
        Staff staff,
        Money money,
        StateRefresh stateRefresh)
    {
        _missionPrep = missionPrep;
        _archive = archive;
        _factions = factions;
        _playerScore = playerScore;
        _staff = staff;
        _money = money;
        _stateRefresh = stateRefresh;
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

    public bool CanLaunchMission(int offset = 0)
    {
        if (_playerScore.GameOver || !CurrentlyAvailable)
            return false;

        if (!WithinRange(_missionPrep.SoldiersToSend) 
            && _missionPrep.SoldiersToSend > _missionPrep.MinSoldiersToSend)
            _missionPrep.NarrowSoldiersToSend();

        return WithinRange(_missionPrep.SoldiersToSend + offset);

        bool WithinRange(int soldiersToSend)
        {
            return _missionPrep.MinSoldiersToSend <= soldiersToSend 
                   && soldiersToSend <= _missionPrep.MaxSoldiersToSend;
        }
    }

    public void LaunchMission()
    {
        Debug.Assert(CanLaunchMission());
        // Roll between 1 and 100.
        // The lower the better.
        int roll = _random.Next(1, 100+1);
        bool success = roll <= SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {SuccessChance} resulting in {(success ? "success" : "failure")}");

        int scoreDiff;
        if (success)
        {
            scoreDiff = Math.Min(PlayerScore.WinScore, Faction.Score);
            _playerScore.Value += scoreDiff;
            Faction.Score -= scoreDiff;
            _money.AddMoney(MoneyReward);
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            _playerScore.Value -= scoreDiff;
            Faction.Score += scoreDiff;
        }

        int soldiersLost = 0;
        for (int i = 0; i < _missionPrep.SoldiersToSend; i++)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int soldierRoll = _random.Next(1, 100+1);
            bool soldierSurvived = soldierRoll <= SoldierSurvivalChance;
            Console.Out.WriteLine(
                $"Soldier {i} {(soldierSurvived ? "survived" : "lost")}. " +
                $"Rolled {soldierRoll} <= {SoldierSurvivalChance}");
            if (!soldierSurvived)
                soldiersLost++;
        }

        if (soldiersLost > 0)
        {
            _archive.RecordLostSoldiers(soldiersLost);
            _staff.SubtractSoldiers(soldiersLost);
        }
        else
        {
            Console.Out.WriteLine("No soldiers lost! \\o/");
        }

        _archive.ArchiveMission(missionSuccessful: success);
        string missionRollReport =
            $" (Rolled {roll} against limit of {SuccessChance}.)";
        string missionSuccessReport = success 
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {Faction.Name} and earned ${MoneyReward}." 
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {Faction.Name}.";
        
        string soldiersLostReport = soldiersLost > 0 ? $"Number of soldiers lost: {soldiersLost}." : "We didn't lose any soldiers.";
        _archive.WriteLastMissionReport($"The last mission was {missionSuccessReport} {soldiersLostReport}");
        GenerateNewOrClearMission();
        _missionPrep.NarrowSoldiersToSend();
        _stateRefresh.Trigger();
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