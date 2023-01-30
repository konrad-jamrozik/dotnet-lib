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
    private readonly Agents _agents;
    private readonly Accounting _accounting;
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly GameState _gameState;
    private readonly MissionOutcome _missionOutcome;

    public MissionLauncher(
        MissionDeployment missionDeployment,
        ArchiveData archiveData,
        PlayerScore playerScore,
        Accounting accounting,
        ViewStateRefresh viewStateRefresh,
        GameState gameState,
        Agents agents,
        MissionOutcome missionOutcome)
    {
        _missionDeployment = missionDeployment;
        _archiveData = archiveData;
        _playerScore = playerScore;
        _accounting = accounting;
        _viewStateRefresh = viewStateRefresh;
        _gameState = gameState;
        _agents = agents;
        _missionOutcome = missionOutcome;
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

    // kja make LaunchMission() functional / immutable
    public void LaunchMission(MissionSite missionSite)
    {
        Debug.Assert(CanLaunchMission(missionSite));
        int successChance = missionSite.MissionStats.SuccessChance;
        int agentsSent = _agents.AgentsAssignedToMission.Count;
        int moneyReward = missionSite.MissionStats.MoneyReward;
        Console.Out.WriteLine($"Sent {agentsSent} agents.");

        (int missionRoll, bool missionSuccessful, List<MissionOutcome.AgentOutcome> agentOutcomes) =
            _missionOutcome.Roll(missionSite.MissionStats, sentAgents: _agents.AgentsAssignedToMission);

        int agentsLost = agentOutcomes.Count(agent => agent.Lost);
        int scoreDiff = ScoreDiff(missionSuccessful, missionSite.FactionData.Score);
        
        WriteLastMissionReport(missionSite, successChance, missionRoll, missionSuccessful, scoreDiff, agentsLost, moneyReward);

        ApplyAgentOutcomes(missionSuccessful, agentOutcomes);
        ApplyMissionOutcome(missionSite, missionSuccessful, scoreDiff);

        _archiveData.ArchiveMission(missionSuccessful);

        
        missionSite.GenerateNewOrClearMission();

        _gameState.Persist();
        _viewStateRefresh.Trigger();
    }

    private void ApplyAgentOutcomes(
        bool missionSuccessful,
        List<MissionOutcome.AgentOutcome> agentOutcomes)
    {
        List<(Agent agent, bool missionSuccess)> lostAgents = new List<(Agent, bool)>();
        foreach (var agentOutcome in agentOutcomes)
        {
            string messageSuffix = "";

            if (agentOutcome.Survived)
            {
                float recovery = agentOutcome.Recovery(missionSuccessful);
                agentOutcome.Agent.RecordMissionOutcome(missionSuccessful, recovery);
                messageSuffix = agentOutcome.Survived ? $" Need {recovery} units of recovery." : "";
            }
            else
            {
                lostAgents.Add((agentOutcome.Agent, missionSuccessful));
            }

            var inequalitySign = agentOutcome.Roll <= agentOutcome.SurvivalChance ? "<=" : ">";
            Console.Out.WriteLine(
                $"Agent #{agentOutcome.Agent.Data.Id} '{agentOutcome.Agent.Data.FullName}' exp: {agentOutcome.ExpBonus} : " +
                $"{(agentOutcome.Survived ? "survived" : "lost")}. " +
                $"Rolled {agentOutcome.Roll} {inequalitySign} {agentOutcome.SurvivalChance}." +
                messageSuffix);
        }

        if (lostAgents.Count > 0)
            _agents.LoseAgents(lostAgents);
        else
            Console.Out.WriteLine("No agents lost! \\o/");
    }
}
