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
    private readonly Timeline _timeline;
    private readonly GameState _gameState;

    public MissionLauncher(
        MissionPrep missionPrep,
        Archive archive,
        PlayerScore playerScore,
        Staff staff,
        Money money,
        StateRefresh stateRefresh,
        GameState gameState,
        Timeline timeline)
    {
        _missionPrep = missionPrep;
        _archive = archive;
        _playerScore = playerScore;
        _staff = staff;
        _money = money;
        _stateRefresh = stateRefresh;
        _gameState = gameState;
        _timeline = timeline;
    }

    private int ApplyMissionOutcome(PendingMission mission, bool success)
    {
        int scoreDiff;
        if (success)
        {
            _money.AddMissionLoot(mission.MoneyReward);
            scoreDiff = Math.Min(PlayerScore.WinScore, mission.Faction.Score);
            _playerScore.Data.Value += scoreDiff;
            mission.Faction.Score -= scoreDiff;
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            _playerScore.Data.Value -= scoreDiff;
            mission.Faction.Score += scoreDiff;
        }

        return scoreDiff;
    }

    private void WriteLastMissionReport(
        PendingMission mission,
        int successChance,
        int roll,
        bool success,
        int scoreDiff,
        int soldiersLost,
        int moneyReward)
    {
        string missionRollReport =
            $" (Rolled {roll} against limit of {successChance}.)";
        string missionSuccessReport = success
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {mission.Faction.Name} " +
              $"and earned ${moneyReward}."
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {mission.Faction.Name}.";

        string soldiersLostReport = soldiersLost > 0
            ? $"Number of soldiers lost: {soldiersLost}."
            : "We didn't lose any soldiers.";
        _archive.WriteLastMissionReport(
            $"The last mission was {missionSuccessReport} {soldiersLostReport}");
    }

    public bool CanLaunchMission(PendingMission mission, int offset = 0)
    {
        if (_playerScore.GameOver || !mission.CurrentlyAvailable)
            return false;

        return WithinRange(_staff.Data.SoldiersAssignedToMissionCount + offset);

        bool WithinRange(int soldiersAssignedToMission)
            => soldiersAssignedToMission >= _missionPrep.MinSoldiersSendableOnMission
               && soldiersAssignedToMission <= _missionPrep.MaxSoldiersSendableOnMission;
    }

    public void LaunchMission(PendingMission mission)
    {
        Debug.Assert(CanLaunchMission(mission));
        var successChance = mission.SuccessChance;
        var soldiersSent = _staff.Data.SoldiersAssignedToMission.Count;
        var moneyReward = mission.MoneyReward;
        Console.Out.WriteLine($"Sent {soldiersSent} soldiers.");
        var (roll, success) = RollMissionOutcome(mission);
        var soldiersLost = ProcessSoldierUpdates(mission, success, _staff.Data.SoldiersAssignedToMission);
        var scoreDiff = ApplyMissionOutcome(mission, success);
        _archive.ArchiveMission(missionSuccessful: success);
        WriteLastMissionReport(mission, successChance, roll, success, scoreDiff, soldiersLost, moneyReward);
        mission.GenerateNewOrClearMission();
        // kja obsolete
        //_missionPrep.NarrowSoldiersToSend();
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

    private int ProcessSoldierUpdates(PendingMission mission, bool missionSuccess, List<Soldier> sentSoldiers)
    {
        List<(Soldier Soldier, int roll, int survivalChance, int expBonus)> soldierData =
            new List<(Soldier Soldier, int roll, int survivalChance, int expBonus)>();

        foreach (Soldier soldier in sentSoldiers)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int soldierRoll = _random.Next(1, 100 + 1);
            var expBonus = soldier.ExperienceBonus(_timeline.CurrentTime);
            var soldierSurvivalChance
                = mission.SoldierSurvivalChance(expBonus);
            soldierData.Add((soldier, soldierRoll, soldierSurvivalChance, expBonus));
        }

        List<(Soldier soldier, int lostTime, bool missionSuccess)> lostSoldiers = new List<(Soldier, int, bool)>();
        foreach (var data in soldierData)
        {
            var (soldier, soldierRoll, soldierSurvivalChance, expBonus) = data;
            bool soldierSurvived = soldierRoll <= soldierSurvivalChance;
            string messageSuffix = "";

            if (soldierSurvived)
            {
                // Higher roll means it was a closer call, so soldier needs more time to recover from fatigue 
                // and wounds. This means that if a soldier is very good at surviving, they may barely survive,
                // but need tons of time to recover.
                var recovery = (float)Math.Round(soldierRoll * (missionSuccess ? 0.5f : 1), 2);
                soldier.RecordMissionOutcome(missionSuccess, recovery);
                messageSuffix = soldierSurvived ? $" Need {recovery} units of recovery." : "";
            }
            else
            {
                lostSoldiers.Add((soldier, _timeline.CurrentTime, missionSuccess));
            }

            var inequalitySign = soldierRoll <= soldierSurvivalChance ? "<=" : ">";
            Console.Out.WriteLine(
                $"Soldier #{soldier.Id} '{soldier.Nickname}' exp: {expBonus} : " +
                $"{(soldierSurvived ? "survived" : "lost")}. " +
                $"Rolled {soldierRoll} {inequalitySign} {soldierSurvivalChance}." +
                messageSuffix);
        }

        if (lostSoldiers.Count > 0)
            _staff.LoseSoldiers(lostSoldiers);
        else
            Console.Out.WriteLine("No soldiers lost! \\o/");

        return lostSoldiers.Count;
    }
}
