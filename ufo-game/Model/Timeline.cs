using Blazored.LocalStorage;

namespace UfoGame.Model;

public class Timeline
{
    public int CurrentTime { get; private set; } = 0;

    private readonly ISyncLocalStorageService  _localStorage;

    public Timeline(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
        // kja experimental
        if (_localStorage.ContainKey("currentTime"))
            CurrentTime = _localStorage.GetItem<int>("currentTime");
    }

    public void IncrementTime()
    {
        CurrentTime += 1;
        Console.Out.WriteLine($"Advanced time to {CurrentTime}");
    }
}