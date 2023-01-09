using System.Diagnostics;

namespace UfoGame.Model;

public class MissionLauncher
{
    private readonly Random _random = new Random();
    private readonly MissionPrep _missionPrep;
    private readonly OperationsArchive _archive;
    private readonly PlayerScore _playerScore;
    private readonly Staff _staff;
    private readonly Money _money;
    private readonly StateRefresh _stateRefresh;
    private readonly GameState _gameState;

    public MissionLauncher(
        MissionPrep missionPrep,
        OperationsArchive archive,
        PlayerScore playerScore,
        Staff staff,
        Money money,
        StateRefresh stateRefresh,
        GameState gameState)
    {
        _missionPrep = missionPrep;
        _archive = archive;
        _playerScore = playerScore;
        _staff = staff;
        _money = money;
        _stateRefresh = stateRefresh;
        _gameState = gameState;
    }

    public bool CanLaunchMission(PendingMission mission, int offset = 0)
    {
        if (_playerScore.GameOver || !mission.CurrentlyAvailable)
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

    public void LaunchMission(PendingMission mission)
    {
        Debug.Assert(CanLaunchMission(mission));
        // Roll between 1 and 100.
        // The lower the better.
        int roll = _random.Next(1, 100+1);
        bool success = roll <= mission.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {mission.SuccessChance} resulting in {(success ? "success" : "failure")}");

        int scoreDiff;
        if (success)
        {
            scoreDiff = Math.Min(PlayerScore.WinScore, mission.Faction.Score);
            _playerScore.Value += scoreDiff;
            mission.Faction.Score -= scoreDiff;
            _money.AddMoney(mission.Data.MoneyReward);
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            _playerScore.Value -= scoreDiff;
            mission.Faction.Score += scoreDiff;
        }

        int soldiersLost = 0;
        for (int i = 0; i < _missionPrep.SoldiersToSend; i++)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int soldierRoll = _random.Next(1, 100+1);
            bool soldierSurvived = soldierRoll <= mission.SoldierSurvivalChance;
            Console.Out.WriteLine(
                $"Soldier {i} {(soldierSurvived ? "survived" : "lost")}. " +
                $"Rolled {soldierRoll} <= {mission.SoldierSurvivalChance}");
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
            $" (Rolled {roll} against limit of {mission.SuccessChance}.)";
        string missionSuccessReport = success 
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {mission.Faction.Name} and earned ${mission.Data.MoneyReward}." 
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {mission.Faction.Name}.";
        
        string soldiersLostReport = soldiersLost > 0 ? $"Number of soldiers lost: {soldiersLost}." : "We didn't lose any soldiers.";
        _archive.WriteLastMissionReport($"The last mission was {missionSuccessReport} {soldiersLostReport}");
        mission.GenerateNewOrClearMission();
        _missionPrep.NarrowSoldiersToSend();
        _gameState.PersistGameState();
        _stateRefresh.Trigger();
    }
}
