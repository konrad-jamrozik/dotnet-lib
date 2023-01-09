using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMission
{
    [JsonInclude]
    public PendingMissionData Data { get; private set; }
    
    //public Faction Faction { get; private set; } = new Faction(name: "placeholder", 0, 0);
    public Faction Faction => _factions.Data.Single(f => f.Name == Data.FactionName);

    public int EnemyPower => (int)(Faction.Score * Data.EnemyPowerCoefficient);

    public int OurPower => _missionPrep.SoldiersToSend * _staff.SoldierEffectiveness;

    public int SuccessChance => Math.Min(100, (int)(OurPower / (float)(EnemyPower + OurPower) * 100));

    public int SoldierSurvivalChance => 
        (int)(SoldierSurvivabilityPower / (float)(EnemyPower + SoldierSurvivabilityPower) * 100);

    private int SoldierSurvivabilityPower => _missionPrep.SoldiersToSend * _staff.SoldierSurvivability;

    public int CountDown => CurrentlyAvailable ? -Data.ExpiresIn : Data.AvailableIn;

    public bool CurrentlyAvailable => Data.AvailableIn == 0 && Data.ExpiresIn > 0;
    
    public bool MissionAboutToExpire => CurrentlyAvailable && Data.ExpiresIn == 1;

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
        Data = GenerateNewMission();
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
                _playerScore.Value -= PlayerScore.IgnoreMissionScoreLoss;
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
        if (!_playerScore.GameOver)
            Data = GenerateNewMission();
        else
            Data = RemoveMission();
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
            _money.AddMoney(Data.MoneyReward);
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
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {Faction.Name} and earned ${Data.MoneyReward}." 
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {Faction.Name}.";
        
        string soldiersLostReport = soldiersLost > 0 ? $"Number of soldiers lost: {soldiersLost}." : "We didn't lose any soldiers.";
        _archive.WriteLastMissionReport($"The last mission was {missionSuccessReport} {soldiersLostReport}");
        GenerateNewOrClearMission();
        _missionPrep.NarrowSoldiersToSend();
        _stateRefresh.Trigger();
    }

    private PendingMissionData GenerateNewMission()
    {
        // kja refactor so there is no placeholder Faction, like Factions.NoFaction; will need to split class ctor into 
        // initial app startup and instance creation. I.e. GenerateNew() should return new instance of 
        // PendingMission instead of assigning fields.
        Debug.Assert(!_playerScore.GameOver);
        return new PendingMissionData(
            availableIn: _random.Next(1, 6 + 1),
            expiresIn: _random.Next(1, 6 + 1),
            moneyReward: _random.Next(10, 200 + 1),
            enemyPowerCoefficient: _random.Next(5, 15 + 1) / (float)10,
            factionName: _factions.RandomUndefeatedFaction.Name);
    }

    private PendingMissionData RemoveMission()
    {
        return new PendingMissionData(0, 0, 0, 1, Factions.NoFaction);
    }
}