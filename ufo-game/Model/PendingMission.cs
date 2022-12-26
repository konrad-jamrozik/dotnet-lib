namespace UfoGame.Model;

public class PendingMission
{
    public int Difficulty { get; private set; }

    private readonly MissionPrep _missionPrep;

    public PendingMission(MissionPrep missionPrep)
    {
        _missionPrep = missionPrep;
    }
    public int SuccessChance => Math.Min(100, 100 + _missionPrep.SoldiersToSend - Difficulty);

    public void RandomizeDifficulty()
    {
        // Difficulty between 0 (guaranteed baseline success)
        // and 100 (guaranteed baseline failure).
        Difficulty = new Random().Next(101);
    }
}