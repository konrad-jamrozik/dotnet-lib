using System.Diagnostics;
using UfoGame.Model.Data;

namespace UfoGame.Model;

public class Procurement
{
    private const int AgentPrice = 50;

    public readonly ProcurementData Data;

    private readonly Accounting _accounting;
    private readonly PlayerScore _playerScore;
    private readonly Agents _agents;

    public Procurement(
        ProcurementData data,
        Accounting accounting,
        PlayerScore playerScore,
        Agents agents)
    {
        Data = data;
        _accounting = accounting;
        _playerScore = playerScore;
        _agents = agents;
    }

    public int AgentsToHireCost => Data.AgentsToHire * AgentPrice;

    public int MinAgentsToHire => 1;

    public int MaxAgentsToHire => _accounting.CurrentMoney / AgentPrice;

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
        _accounting.PayForHiringAgents(AgentsToHireCost);
        _agents.HireAgents(Data.AgentsToHire);
    }
}