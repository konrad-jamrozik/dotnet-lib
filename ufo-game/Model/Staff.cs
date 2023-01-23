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

    public void LoseAgents(List<(Agent agent, int lostTime, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.lostTime, data.missionSuccess));
        _archive.RecordLostAgents(agents.Count);
    }
}

