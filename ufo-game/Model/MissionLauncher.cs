using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class MissionLauncher
{
    private readonly Random _random = new Random();
    private readonly MissionDeployment _missionDeployment;
    private readonly ArchiveData _archiveData;
    private readonly PlayerScore _playerScore;
    private readonly Agents _agents;
    private readonly Accounting _accounting;
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameState _gameState;

    public MissionLauncher(
        MissionDeployment missionDeployment,
        ArchiveData archiveData,
        PlayerScore playerScore,
        Accounting accounting,
        ViewStateRefresh viewStateRefresh,
        GameState gameState,
        Agents agents)
    {
        _missionDeployment = missionDeployment;
        _archiveData = archiveData;
        _playerScore = playerScore;
        _accounting = accounting;
        _viewStateRefresh = viewStateRefresh;
        _gameState = gameState;
        _agents = agents;
    }

    private int ApplyMissionOutcome(MissionSite missionSite, bool success)
    {
        int scoreDiff;
        if (success)
        {
            _accounting.AddMissionLoot(missionSite.MoneyReward);
            scoreDiff = Math.Min(PlayerScore.WinScore, missionSite.FactionData.Score);
            _playerScore.Data.Value += scoreDiff;
            missionSite.FactionData.Score -= scoreDiff;
        }
        else
        {
            scoreDiff = PlayerScore.LoseScore;
            _playerScore.Data.Value -= scoreDiff;
            missionSite.FactionData.Score += scoreDiff;
        }

        return scoreDiff;
    }

    private void WriteLastMissionReport(
        MissionSite missionSite,
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
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {missionSite.FactionData.Name} " +
              $"and earned ${moneyReward}."
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {missionSite.FactionData.Name}.";

        string agentsLostReport = agentsLost > 0
            ? $"Number of agents lost: {agentsLost}."
            : "We didn't lose any agents.";
        _archiveData.WriteLastMissionReport(
            $"The last mission was {missionSuccessReport} {agentsLostReport}");
    }

    public bool CanLaunchMission(MissionSite missionSite, int offset = 0)
    {
        if (_playerScore.GameOver || !missionSite.CurrentlyAvailable)
            return false;

        return WithinRange(_agents.AgentsAssignedToMissionCount + offset);

        bool WithinRange(int agentsAssignedToMission)
            => agentsAssignedToMission >= _missionDeployment.MinAgentsSendableOnMission
               && agentsAssignedToMission <= _missionDeployment.MaxAgentsSendableOnMission;
    }

    // kja make LaunchMission() functional / immutable, to avoid unexpected mutations
    public void LaunchMission(MissionSite missionSite)
    {
        Debug.Assert(CanLaunchMission(missionSite));
        var successChance = missionSite.SuccessChance;
        var agentsSent = _agents.AgentsAssignedToMission.Count;
        var moneyReward = missionSite.MoneyReward;
        Console.Out.WriteLine($"Sent {agentsSent} agents.");
        var (roll, success) = RollMissionOutcome(missionSite);
        var agentsLost = ProcessAgentUpdates(missionSite, success, _agents.AgentsAssignedToMission);
        var scoreDiff = ApplyMissionOutcome(missionSite, success);
        _archiveData.ArchiveMission(missionSuccessful: success);
        WriteLastMissionReport(missionSite, successChance, roll, success, scoreDiff, agentsLost, moneyReward);
        missionSite.GenerateNewOrClearMission();

        _gameState.Persist();
        _viewStateRefresh.Trigger();
    }

    // kja introduce class like "MissionOutcome" which will have method like "roll" and
    // many of the stats currently on MissionSite
    private (int roll, bool success) RollMissionOutcome(MissionSite missionSite)
    {
        // Roll between 1 and 100.
        // The lower the better.
        int roll = _random.Next(1, 100 + 1);
        bool success = roll <= missionSite.SuccessChance;
        Console.Out.WriteLine(
            $"Rolled {roll} against limit of {missionSite.SuccessChance} resulting in {(success ? "success" : "failure")}");
        return (roll, success);
    }

    private int ProcessAgentUpdates(MissionSite missionSite, bool missionSuccess, List<Agent> sentAgents)
    {
        List<(Agent agent, int roll, int survivalChance, int expBonus)> agentData =
            new List<(Agent agent, int roll, int survivalChance, int expBonus)>();

        foreach (Agent agent in sentAgents)
        {
            // Roll between 1 and 100.
            // The lower the better.
            int agentRoll = _random.Next(1, 100 + 1);
            var expBonus = agent.ExperienceBonus();
            var agentSurvivalChance
                = missionSite.AgentSurvivalChance(expBonus);
            agentData.Add((agent, agentRoll, agentSurvivalChance, expBonus));
        }

        List<(Agent agent, bool missionSuccess)> lostAgents = new List<(Agent, bool)>();
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
                lostAgents.Add((agent, missionSuccess));
            }

            var inequalitySign = agentRoll <= agentSurvivalChance ? "<=" : ">";
            Console.Out.WriteLine(
                $"Agent #{agent.Data.Id} '{agent.Data.FullName}' exp: {expBonus} : " +
                $"{(agentSurvived ? "survived" : "lost")}. " +
                $"Rolled {agentRoll} {inequalitySign} {agentSurvivalChance}." +
                messageSuffix);
        }

        if (lostAgents.Count > 0)
            _agents.LoseAgents(lostAgents);
        else
            Console.Out.WriteLine("No agents lost! \\o/");

        return lostAgents.Count;
    }
}
