using System.Diagnostics;

namespace UfoGame.Model;

public class PendingMission
{
    public int Difficulty { get; private set; }

    public int AvailableIn { get; private set; }

    public int ExpiresIn { get; private set; }

    public bool CurrentlyAvailable => AvailableIn == 0 && ExpiresIn > 0;

    private readonly MissionPrep _missionPrep;

    public PendingMission(MissionPrep missionPrep)
    {
        _missionPrep = missionPrep;
        GenerateNew();
    }

    // kja this should be in a separate type
    public int SuccessChance => Math.Min(100, 100 + _missionPrep.SoldiersToSend - Difficulty);

    public void AdvanceTime()
    {
        Console.Out.WriteLine("PendingMission - AdvanceTime");
        if (CurrentlyAvailable)
        {
            Debug.Assert(ExpiresIn >= 1);
            if (ExpiresIn == 1)
                GenerateNew();
            else
                ExpiresIn--;
        }
        else
        {
            Debug.Assert(AvailableIn >= 1);
            AvailableIn--;
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
    }
}