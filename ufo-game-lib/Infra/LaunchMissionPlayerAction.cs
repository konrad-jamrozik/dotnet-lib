using System.Runtime.CompilerServices;
using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class LaunchMissionPlayerAction : PlayerAction
{
    private readonly MissionSite _site;
    public int AgentCount { get; }

    public LaunchMissionPlayerAction(MissionSite site, int agentCount)
    {
        _site = site;
        AgentCount = agentCount;
    }

    public override void Apply(GameState state)
    {
        Debug.Assert(state.MissionSites.Contains(_site));
        Debug.Assert(state.Assets.CurrentTransportCapacity >= AgentCount);
        Console.Out.WriteLine($"Launch mission. SiteId: {_site.Id} AgentCount: {AgentCount}");
        // kja need to decrease TransportCapacity by the agents sent until mission is completed (for now it just means time is advanced)
        state.Missions.Add(new Mission(_site));
        state.MissionSites.Single(site => site.Id == _site.Id).IsActive = false;
        state.Assets.CurrentTransportCapacity -= AgentCount;
    }
}