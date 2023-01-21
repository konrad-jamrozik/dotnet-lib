using Blazored.LocalStorage;

namespace UfoGame.Infra;

public class PersistentStorage
{
    private readonly ISyncLocalStorageService _localStorage;

    public PersistentStorage(ISyncLocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public bool HasSavedGame => _localStorage.ContainKey(nameof(GameState));

    public void SetItem<T>(string key, T data)
    {
        _localStorage.SetItem(key, data);
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