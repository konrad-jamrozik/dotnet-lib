using System.Diagnostics;

namespace UfoGame.Model;

public class PendingMission
{
    public int Difficulty { get; private set; }

    public int AvailableIn { get; private set; }

    public int ExpiresIn { get; private set; }

    public bool CurrentlyAvailable => AvailableIn == 0 && ExpiresIn > 0;

    // kja refactor so there is no placeholder; will need to split class ctor into 
    // initial app startup and instance creation. I.e. GenerateNew() should return new instance of 
    // PendingMission instead of assigning fields.
    public Faction Faction { get; private set; } = new Faction(name: "placeholder", 0, 0);

    private readonly MissionPrep _missionPrep;
    private readonly OperationsArchive _archive;
    private readonly Factions _factions;
    private readonly PlayerScore _playerScore;

    public PendingMission(MissionPrep missionPrep, OperationsArchive archive, Factions factions, PlayerScore playerScore)
    {
        _missionPrep = missionPrep;
        _archive = archive;
        _factions = factions;
        _playerScore = playerScore;
        GenerateNew();
    }

    // kja this should be in a separate type
    public int SuccessChance => Math.Min(100, 100 + _missionPrep.SoldiersToSend * 5 - Difficulty);

    public void AdvanceMissionTime()
    {
        Console.Out.WriteLine("PendingMission - AdvanceTime");
        if (CurrentlyAvailable)
        {
            Debug.Assert(ExpiresIn >= 1);
            if (ExpiresIn == 1)
            {
                _archive.RecordIgnoredMission();
                // kja need to output it somewhere so player knows why they lost score
                _playerScore.Value -= PlayerScore.IgnoreMissionScoreLoss;
                GenerateNew();
            }
            else
                ExpiresIn--;
        }
        else
        {
            Debug.Assert(AvailableIn >= 1);
            AvailableIn--;
            if (CurrentlyAvailable)
            {
                if (!Faction.Discovered)
                {
                    Console.Out.WriteLine("Discovered faction! " + Faction.Name);
                    Faction.Discovered = true;
                }
            }
        }
    }

    public void GenerateNew()
    {
        var random = new Random();
        AvailableIn = random.Next(1, 4); // random.Next(3, 11);
        ExpiresIn = random.Next(1, 4);
        // Difficulty between 0 (guaranteed baseline success)
        // and 100 (guaranteed baseline failure).
        Difficulty = random.Next(101);

        // kja this won't properly handle a case where all factions were defeated.
        // In such case, the game should end anyway.
        // It will throw index OOB on line 78.
        var undefeatedFactions = _factions.Data.Where(faction => !faction.Defeated).ToArray();
        // For now just randomize
        Faction = undefeatedFactions[random.Next(undefeatedFactions.Length)];
    }
}