using System.Diagnostics;
using UfoGame.Model;
using UfoGame.Model.Data;

namespace UfoGame.ViewModel;

class LaunchMissionPlayerAction : IPlayerActionOnRangeInput
{
    private readonly Agents _agents;
    private readonly MissionDeployment _missionDeployment;
    private readonly MissionSite _missionSite;
    private readonly MissionLauncher _missionLauncher;
    private readonly TimelineData _timelineData;
    private readonly ViewStateRefresh _viewStateRefresh;

    public LaunchMissionPlayerAction(
        MissionDeployment missionDeployment,
        MissionSite missionSite,
        ViewStateRefresh viewStateRefresh,
        MissionLauncher missionLauncher,
        TimelineData timelineData,
        Agents agents)
    {
        _missionDeployment = missionDeployment;
        _missionSite = missionSite;
        _viewStateRefresh = viewStateRefresh;
        _missionLauncher = missionLauncher;
        _timelineData = timelineData;
        _agents = agents;
    }

    public void Act() => _missionLauncher.LaunchMission(_missionSite);

    public string ActLabel()
        => $"Launch with {_agents.AgentsAssignedToMissionCount} agents";

    // Range input is permanently disabled for assigning agents to mission.
    public bool CanSetRangeInput => false;

    public bool CanDecrementInput => _agents.AgentsAssignedToMissionCount > 0;

    public int Input
    {
        get => _agents.AgentsAssignedToMissionCount;
        // ReSharper disable once ValueParameterNotUsed
        set => Debug.Assert(false, 
            "Range input is permanently disabled for assigning agents to mission.");
    }

    public void IncrementInput()
    {
        var assignableAgents = _agents
            .AssignableAgentsSortedByLaunchPriority(_timelineData.CurrentTime);
        Debug.Assert(assignableAgents.Any());
        assignableAgents.First().AssignToMission();
        _viewStateRefresh.Trigger();
    }

    public void DecrementInput()
    {
        var assignedAgents = _agents
            .AssignedAgentsSortedByDescendingLaunchPriority(_timelineData.CurrentTime);
        Debug.Assert(assignedAgents.Any());
        assignedAgents.First().UnassignFromMission();
        _viewStateRefresh.Trigger();
    }

    public bool CanAct() => _missionLauncher.CanLaunchMission(_missionSite);

    public bool CanAct(int value) => _missionLauncher.CanLaunchMission(_missionSite, value);

    public int InputMax() => _missionDeployment.MaxAgentsSendableOnMission;

    public int InputMin() => _missionDeployment.MinAgentsSendableOnMission;
}