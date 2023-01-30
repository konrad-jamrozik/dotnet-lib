using UfoGame.Model;

namespace UfoGame.ViewModel;

public class SackAgentPlayerAction : IPlayerActionOnButton
{
    private readonly ViewStateRefresh _viewStateRefresh;
    private readonly Agents _agents;
    private readonly Agent _agent;

    public SackAgentPlayerAction(ViewStateRefresh viewStateRefresh, Agents agents, Agent agent)
    {
        _viewStateRefresh = viewStateRefresh;
        _agents = agents;
        _agent = agent;
    }

    public string ActLabel()
        => "Sack agent";

    public bool CanAct()
        => _agent.CanSack;

    // kja Act() should first display a modal. Need something like IPlayerActionOnButtonWithModal.
    // See "settings.razor" for inspiration how to do it.
    public void Act()
    {
        _agents.SackAgent(_agent);
        _viewStateRefresh.Trigger();
    }
}