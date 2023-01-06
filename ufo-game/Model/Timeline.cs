using Blazored.LocalStorage;

namespace UfoGame.Model;

public class Timeline
{
    public int CurrentTime { get; private set; } = 0;

    public Timeline(PersistentStorage storage)
    {
        // kja experimental
        if (storage.ContainKey("currentTime"))
            CurrentTime = storage.GetItem<int>("currentTime");
    }

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