using System.Diagnostics;
using System.Text.Json.Serialization;

namespace UfoGame.Model;

public class PendingMission
{
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

    private readonly Random _random = new Random();
    private readonly MissionPrep _missionPrep;
    private readonly OperationsArchive _archive;
    private readonly Factions _factions;
    private readonly PlayerScore _playerScore;
    private readonly Staff _staff;

    public PendingMission(
        PendingMissionData data,
        MissionPrep missionPrep,
        OperationsArchive archive,
        Factions factions,
        PlayerScore playerScore,
        Staff staff)
    {
        Data = data;
        _missionPrep = missionPrep;
        _archive = archive;
        _factions = factions;
        _playerScore = playerScore;
        _staff = staff;
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
}