using Blazored.LocalStorage;

namespace UfoGame.Model;

// kja move this and other supporting infra classes to different namespace than "model"
public class PersistentStorage
{
    private readonly ISyncLocalStorageService _localStorage;

    public PersistentStorage(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public bool HasSavedGame => _localStorage.ContainKey(nameof(Game));

    public void PersistGameState(Game game)
    {
        Console.Out.WriteLine("Persisting game state");
        var itemsToSave = new List<(string key, object value)>
        {
            ("Game", game),
            // ("Timeline", Timeline),
            // ("PendingMission", PendingMission)
        };
        // foreach (var faction in Factions.Data)
        // {
        //     itemsToSave.Add(($"Faction.Name:\"{faction.Name}\"", faction));
        // }
        foreach (var item in itemsToSave)
        {
            Console.Out.WriteLine("Persisting item: " + item.key + " : " + item.value);
            SetItem(item.key, item.value);
        }
    }


    public void SetItem<T>(string key, T data)
    {
        _localStorage.SetItem(key, data);
    }

    public bool ContainKey(string key)
    {
        return _localStorage.ContainKey(key);
    }

    public T GetItem<T>(string key)
    {
        return _localStorage.GetItem<T>(key);
    }

    public string GetItemAsString(string key)
    {
        return _localStorage.GetItemAsString(key);
    }

    public void Reset()
    {
        _localStorage.Clear();
    }
}