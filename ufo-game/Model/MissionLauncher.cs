using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

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
        int agentsLost,
        int moneyReward)
    {
        string missionRollReport =
            $" (Rolled {roll} against limit of {successChance}.)";
        string missionSuccessReport = success
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {mission.Faction.Name} " +
              $"and earned ${moneyReward}."
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {mission.Faction.Name}.";

        string agentsLostReport = agentsLost > 0
            ? $"Number of agents lost: {agentsLost}."
            : "We didn't lose any agents.";
        _archive.WriteLastMissionReport(
            $"The last mission was {missionSuccessReport} {agentsLostReport}");
    }

    public bool CanLaunchMission(PendingMission mission, int offset = 0)
    {
        if (_playerScore.GameOver || !mission.CurrentlyAvailable)
            return false;

        return WithinRange(_staff.Data.AgentsAssignedToMissionCount + offset);

        bool WithinRange(int agentsAssignedToMission)
            => agentsAssignedToMission >= _missionPrep.MinAgentsSendableOnMission
               && agentsAssignedToMission <= _missionPrep.MaxAgentsSendableOnMission;
    }

    // kja make it functional / immutable, to avoid unexpected mutations
    public void LaunchMission(PendingMission mission)
    {
        Debug.Assert(CanLaunchMission(mission));
        var successChance = mission.SuccessChance;
        var agentsSent = _staff.Data.AgentsAssignedToMission.Count;
        var moneyReward = mission.MoneyReward;
        Console.Out.WriteLine($"Sent {agentsSent} agents.");
        var (roll, success) = RollMissionOutcome(mission);
        var agentsLost = ProcessAgentUpdates(mission, success, _staff.Data.AgentsAssignedToMission);
        var scoreDiff = ApplyMissionOutcome(mission, success);
        _archive.ArchiveMission(missionSuccessful: success);
        WriteLastMissionReport(mission, successChance, roll, success, scoreDiff, agentsLost, moneyReward);
        mission.GenerateNewOrClearMission();
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

    private int ProcessAgentUpdates(PendingMission mission, bool missionSuccess, List<Agent> sentAgents)
    {
        List<(Agent agent, int roll, int survivalChance, int expBonus)> agentData =
            new List<(Agent agent, int roll, int survivalChance, int expBonus)>();

        foreach (Agent agent in sentAgents)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int agentRoll = _random.Next(1, 100 + 1);
            var expBonus = agent.ExperienceBonus(_timeline.CurrentTime);
            var agentSurvivalChance
                = mission.AgentSurvivalChance(expBonus);
            agentData.Add((agent, agentRoll, agentSurvivalChance, expBonus));
        }

        List<(Agent agent, int lostTime, bool missionSuccess)> lostAgents = new List<(Agent, int, bool)>();
        foreach (var data in agentData)
        {
            var (agent, agentRoll, agentSurvivalChance, expBonus) = data;
            bool agentSurvived = agentRoll <= agentSurvivalChance;
            string messageSuffix = "";

            if (agentSurvived)
            {
                // Higher roll means it was a closer call, so agent needs more time to recover from fatigue 
                // and wounds. This means that if a agent is very good at surviving, they may barely survive,
                // but need tons of time to recover.
                var recovery = (float)Math.Round(agentRoll * (missionSuccess ? 0.5f : 1), 2);
                agent.RecordMissionOutcome(missionSuccess, recovery);
                messageSuffix = agentSurvived ? $" Need {recovery} units of recovery." : "";
            }
            else
            {
                lostAgents.Add((agent, _timeline.CurrentTime, missionSuccess));
            }

            var inequalitySign = agentRoll <= agentSurvivalChance ? "<=" : ">";
            Console.Out.WriteLine(
                $"Agent #{agent.Id} '{agent.Nickname}' exp: {expBonus} : " +
                $"{(agentSurvived ? "survived" : "lost")}. " +
                $"Rolled {agentRoll} {inequalitySign} {agentSurvivalChance}." +
                messageSuffix);
        }

        if (lostAgents.Count > 0)
            _staff.LoseAgents(lostAgents);
        else
            Console.Out.WriteLine("No agents lost! \\o/");

        return lostAgents.Count;
    }
}
