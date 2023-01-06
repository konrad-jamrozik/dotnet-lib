namespace UfoGame.Model;

public class Timeline
{
    public int CurrentTime { get; set; } = 0;

    public void IncrementTime()
    {
        CurrentTime += 1;
        Console.Out.WriteLine($"Advanced time to {CurrentTime}");
    }

    public void Reset()
    {
        CurrentTime = 0;
    }
}