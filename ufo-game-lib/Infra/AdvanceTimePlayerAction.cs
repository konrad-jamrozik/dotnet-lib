using UfoGameLib.Model;

namespace UfoGameLib.Infra;

public class AdvanceTimePlayerAction : PlayerAction
{
    public override void Apply(GameState state)
    {
        state.Timeline.CurrentTurn++;
        Console.Out.WriteLine($"Advance time into turn: {state.Timeline.CurrentTurn}");
        state.Assets.CurrentTransportCapacity = state.Assets.MaxTransportCapacity;
        state.Assets.CurrentMoney -= state.Assets.Agents.Count * 5;

        if (state.Timeline.CurrentTurn % 3 == 0)
        {
            int siteId = state.NextMissionSiteId;
            Console.Out.WriteLine($"Add MissionSite with Id: {siteId}");
            state.MissionSites.Add(new MissionSite(siteId, IsActive: true));
        }
    }
}