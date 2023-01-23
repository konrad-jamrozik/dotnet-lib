using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Staff
{
    public readonly StaffData Data;

    private readonly Archive _archive;

    public Staff(StaffData data, Archive archive)
    {
        Data = data;
        _archive = archive;
    }

    // kja move this to Agents
    public void LoseAgents(List<(Agent agent, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.missionSuccess));
        _archive.RecordLostAgents(agents.Count);
    }
}

