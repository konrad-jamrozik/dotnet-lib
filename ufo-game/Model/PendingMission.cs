using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMission
{
    // kja if this would be injected in ctor, I wouldn't have to ctor this class by hand.
    [JsonInclude]
    public PendingMissionData Data { get; set; } // kja setter made public for serialization only. Fix.
    
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