using System.Diagnostics;

namespace UfoGame.Model;

public class MissionLauncher
{
    private readonly Random _random = new Random();
    private readonly MissionPrep _missionPrep;
    private readonly Archive _archive;
    private readonly PlayerScore _playerScore;
    private readonly Staff _staff;
    private readonly Money _money;
    private readonly StateRefresh _stateRefresh;
    private readonly GameState _gameState;

    public MissionLauncher(
        MissionPrep missionPrep,
        Archive archive,
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

        if (!WithinRange(_missionPrep.Data.SoldiersToSend) 
            && _missionPrep.Data.SoldiersToSend > _missionPrep.MinSoldiersToSend)
            _missionPrep.NarrowSoldiersToSend();

        return WithinRange(_missionPrep.Data.SoldiersToSend + offset);

        bool WithinRange(int soldiersToSend)
        {
            return _missionPrep.MinSoldiersToSend <= soldiersToSend 
                   && soldiersToSend <= _missionPrep.MaxSoldiersToSend;
        }
    }

    public void LaunchMission(PendingMission mission)
    {
        Debug.Assert(CanLaunchMission(mission));
        var soldiersSent = _missionPrep.Data.SoldiersToSend;
        var (roll, success) = RollMissionOutcome(mission);
        var scoreDiff = ApplyMissionOutcome(mission, success);
        var soldiersLost = ProcessSoldierLosses(mission, soldiersSent);

        _archive.ArchiveMission(missionSuccessful: success);
        WriteLastMissionReport(mission, roll, success, scoreDiff, soldiersLost);
        mission.GenerateNewOrClearMission();
        _missionPrep.NarrowSoldiersToSend();
        _gameState.PersistGameState();
        _stateRefresh.Trigger();
    }

    private (int roll, bool success) RollMissionOutcome(PendingMission mission)
    {
        // Roll between 1 and 100.
        // The lower the better.
        int roll = _random.Next(1, 100 + 1);
        bool success = roll <= mission.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {mission.SuccessChance} resulting in {(success ? "success" : "failure")}");
        return (roll, success);
    }

    private int ApplyMissionOutcome(PendingMission mission, bool success)
    {
        int scoreDiff;
        if (success)
        {
            scoreDiff = Math.Min(PlayerScore.WinScore, mission.Faction.Score);
            _playerScore.Data.Value += scoreDiff;
            mission.Faction.Score -= scoreDiff;
            _money.AddMissionLoot(mission.Data.MoneyReward);
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            _playerScore.Data.Value -= scoreDiff;
            mission.Faction.Score += scoreDiff;
        }

        return scoreDiff;
    }

    private int ProcessSoldierLosses(PendingMission mission, int soldiersSent)
    {
        int soldiersLost = 0;
        for (int i = 0; i < soldiersSent; i++)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int soldierRoll = _random.Next(1, 100 + 1);
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
            _staff.LoseSoldiers(soldiersLost);
        }
        else
        {
            Console.Out.WriteLine("No soldiers lost! \\o/");
        }

        _staff.Data.AddRecoveringSoldiers(soldiersSent - soldiersLost);

        return soldiersLost;
    }

    private void WriteLastMissionReport(
        PendingMission mission,
        int roll,
        bool success,
        int scoreDiff,
        int soldiersLost)
    {
        string missionRollReport =
            $" (Rolled {roll} against limit of {mission.SuccessChance}.)";
        string missionSuccessReport = success
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {mission.Faction.Name} and earned ${mission.Data.MoneyReward}."
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {mission.Faction.Name}.";

        string soldiersLostReport = soldiersLost > 0
            ? $"Number of soldiers lost: {soldiersLost}."
            : "We didn't lose any soldiers.";
        _archive.WriteLastMissionReport(
            $"The last mission was {missionSuccessReport} {soldiersLostReport}");
    }
}
