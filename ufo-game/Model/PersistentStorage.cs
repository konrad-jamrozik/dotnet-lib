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

    public void Reset()
    {
        _localStorage.Clear();
    }
}