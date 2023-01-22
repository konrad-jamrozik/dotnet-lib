using System.Diagnostics;
using UfoGame.Model.Data;
using Archive = UfoGame.Model.Data.Archive;

namespace UfoGame.Model;

public class Procurement
{
    public readonly ProcurementData Data;
    
    private const int AgentPrice = 50;

    public int AgentsToHireCost => Data.AgentsToHire * AgentPrice;

    public int MinAgentsToHire => 1;

    public int MaxAgentsToHire => _accounting.CurrentMoney / AgentPrice;

    private readonly Accounting _accounting;
    private readonly Staff _staff;
    private readonly PlayerScore _playerScore;
    private readonly Archive _archive;
    private readonly Timeline _timeline;

    public Procurement(
        ProcurementData data,
        Accounting accounting,
        Staff staff,
        PlayerScore playerScore,
        Archive archive,
        Timeline timeline)
    {
        Data = data;
        _accounting = accounting;
        _staff = staff;
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
        _accounting.PayForHiringAgents(AgentsToHireCost);
        _archive.RecordHiredAgents(Data.AgentsToHire);
        _staff.Data.AddNewRandomAgents(Data.AgentsToHire, _timeline.CurrentTime);
        
    }
}