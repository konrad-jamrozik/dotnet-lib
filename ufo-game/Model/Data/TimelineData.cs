using System.Text.Json.Serialization;

namespace UfoGame.Model.Data;

public class TimelineData : ITemporal, IPersistable, IResettable
{
    [JsonInclude] public int CurrentTime { get; private set; }

    public TimelineData()
        => Reset();

    public void Reset()
        => CurrentTime = 0;

    public void AdvanceTime()
    {
        CurrentTime += 1;
        Console.Out.WriteLine($"Advanced time to {CurrentTime}");
    }
}