using System.Diagnostics;
using System.Text.Json.Serialization;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Staff
{
    [JsonInclude]
    public readonly StaffData Data;

    private const int AgentPrice = 50;

    public int AgentsToHireCost => Data.AgentsToHire * AgentPrice;

    public int MinAgentsToHire => 1;

    public int MaxAgentsToHire => _money.CurrentMoney / AgentPrice;

    private readonly Money _money;
    private readonly Archive _archive;
    private readonly PlayerScore _playerScore;
    private readonly Timeline _timeline;

    public Staff(
        StaffData data,
        Money money,
        PlayerScore playerScore,
        Archive archive,
        Timeline timeline)
    {
        Data = data;
        _money = money;
        _playerScore = playerScore;
        _archive = archive;
        _timeline = timeline;
    }

    public bool CanHireAgents(int offset = 0, bool tryNarrow = true)
    {
        if (_playerScore.GameOver)
            return false;

        if (WithinRange(Data.AgentsToHire + offset))
            return true;

        if (!tryNarrow || Data.AgentsToHire <= MinAgentsToHire)
            return false;

        NarrowAgentsToHire();

        return WithinRange(Data.AgentsToHire + offset);

        bool WithinRange(int agentsToHire)
            => MinAgentsToHire <= agentsToHire && agentsToHire <= MaxAgentsToHire;

        void NarrowAgentsToHire()
            => Data.AgentsToHire = Math.Max(MinAgentsToHire, Math.Min(Data.AgentsToHire, MaxAgentsToHire));
    }

    public void HireAgents()
    {
        Debug.Assert(CanHireAgents(tryNarrow: false));
        _money.PayForHiringAgents(AgentsToHireCost);
        _archive.RecordHiredAgents(Data.AgentsToHire);
        Data.CurrentAgents += Data.AgentsToHire;
        Data.HireAgents(_timeline.CurrentTime);
        Console.Out.WriteLine($"Hired {Data.AgentsToHire} agents. Agents now at {Data.CurrentAgents}.");
    }

    public void AdvanceTime()
    {
        Data.AdvanceTime();
    }

    public void LoseAgents(List<(Agent agent, int lostTime, bool missionSuccess)> agents)
    {
        agents.ForEach(data => data.agent.SetAsLost(data.lostTime, data.missionSuccess));
        _archive.RecordLostAgents(agents.Count);
    }
}

