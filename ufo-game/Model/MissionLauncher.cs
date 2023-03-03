using System.Diagnostics;
using UfoGame.Infra;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Model;

public class MissionLauncher
{
    private readonly MissionDeployment _missionDeployment;
    private readonly ArchiveData _archiveData;
    private readonly PlayerScore _playerScore;
    private readonly Accounting _accounting;
    private readonly Agents _agents;
    private readonly MissionOutcome _missionOutcome;
    private readonly MissionEventLogsData _missionEventLogsData;
    private readonly GameState _gameState;
    private readonly ViewStateRefresh _viewStateRefresh;

    public MissionLauncher(
        MissionDeployment missionDeployment,
        ArchiveData archiveData,
        PlayerScore playerScore,
        Accounting accounting,
        Agents agents,
        MissionOutcome missionOutcome,
        MissionEventLogsData missionEventLogsData,
        GameState gameState,
        ViewStateRefresh viewStateRefresh)
    {
        _missionDeployment = missionDeployment;
        _archiveData = archiveData;
        _playerScore = playerScore;
        _accounting = accounting;
        _agents = agents;
        _missionOutcome = missionOutcome;
        _missionEventLogsData = missionEventLogsData;
        _gameState = gameState;
        _viewStateRefresh = viewStateRefresh;
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

    public void LaunchMission(MissionSite missionSite)
    {
        Debug.Assert(CanLaunchMission(missionSite));
        _missionEventLogsData.Reset();
        _missionEventLogsData.LogAgentsSent(_agents);

        (int missionRoll, bool missionSuccessful, List<MissionOutcome.AgentOutcome> agentOutcomes) =
            _missionOutcome.Roll(missionSite.MissionStats, sentAgents: _agents.AgentsAssignedToMission);

        LogAgentOutcomes(agentOutcomes, missionSuccessful);

        int scoreDiff = ScoreDiff(missionSuccessful, factionScore: missionSite.FactionData.Score);

        WriteMissionReport(
            factionName: missionSite.FactionData.Name,
            successChance: missionSite.MissionStats.SuccessChance,
            missionRoll,
            missionSuccessful,
            scoreDiff,
            agentsLost: agentOutcomes.Count(agent => agent.Lost),
            moneyReward: missionSite.MissionStats.MoneyReward);

        ApplyAgentOutcomes(missionSuccessful, agentOutcomes);
        ApplyMissionOutcome(missionSite, missionSuccessful, scoreDiff);

        _archiveData.ArchiveMission(missionSuccessful);

        missionSite.GenerateNewOrClearMission();

        _gameState.Persist();
        _viewStateRefresh.Trigger();
    }

    private void LogAgentOutcomes(List<MissionOutcome.AgentOutcome> agentOutcomes, bool missionSuccessful)
    {
        foreach (var agentOutcome in agentOutcomes)
            _missionEventLogsData.LogAgentOutcome(missionSuccessful, agentOutcome);

        if (!agentOutcomes.Any(outcome => outcome.Lost))
            _missionEventLogsData.Add("No agents lost! \\o/");
    }

    private void ApplyMissionOutcome(MissionSite missionSite, bool missionSuccessful, int scoreDiff)
    {
        if (missionSuccessful)
            _accounting.AddMissionLoot(missionSite.MissionStats.MoneyReward);

        _playerScore.Data.Value += scoreDiff;
        missionSite.FactionData.Score -= scoreDiff;
    }

    private int ScoreDiff(bool missionSuccessful, int factionScore)
        => missionSuccessful ? Math.Min(PlayerScore.WinScore, factionScore) : PlayerScore.LoseScore;

    private void WriteMissionReport(
        string factionName,
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
            ? $"successful! {missionRollReport} We took {scoreDiff} score from {factionName} " +
              $"and earned ${moneyReward}."
            : $"a failure. {missionRollReport} We lost {scoreDiff} score to {factionName}.";

        string agentsLostReport = agentsLost > 0
            ? $"Number of agents lost: {agentsLost}."
            : "We didn't lose any agents.";
        
        string missionReport = $"The last mission was {missionSuccessReport} {agentsLostReport}";
        _archiveData.WriteLastMissionReport(missionReport);

        string missionEventSummary =
            $"The mission was a {(success ? "success" : "failure")}. " +
            $"Score change: {(success ? "" : "-")}{scoreDiff}. " +
            $"Agents lost: {agentsLost}.";

        _missionEventLogsData.LogMissionReport(missionEventSummary, missionReport);
    }

    private void ApplyAgentOutcomes(
        bool missionSuccessful,
        List<MissionOutcome.AgentOutcome> agentOutcomes)
    {
        List<(Agent agent, bool missionSuccess)> lostAgents = new List<(Agent, bool)>();
        foreach (MissionOutcome.AgentOutcome agentOutcome in agentOutcomes)
        {
            if (agentOutcome.Survived)
            {
                float recovery = agentOutcome.Recovery(missionSuccessful);
                // As of 2/4/2023 We always unassign agents, even if they have recovery == 0,
                // because once a mission is completed, the next one is not yet available,
                // so no agent can be assigned to it.
                agentOutcome.Agent.UnassignFromMission();
                agentOutcome.Agent.RecordMissionOutcome(missionSuccessful, recovery);
            }
            else
            {
                lostAgents.Add((agentOutcome.Agent, missionSuccessful));
            }
        }

        if (lostAgents.Count > 0)
            _agents.LoseAgents(lostAgents);
    }
}
