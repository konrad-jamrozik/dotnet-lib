namespace UfoGame.Model;

public class PendingMission
{
    public int Difficulty { get; private set; } = 0;

    public void RandomizeDifficulty()
    {
        Difficulty = new Random().Next(101);
    }
}