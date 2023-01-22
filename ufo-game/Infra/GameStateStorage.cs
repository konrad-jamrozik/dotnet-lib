using Blazored.LocalStorage;
using System.Text.Json.Nodes;

namespace UfoGame.Infra;

/// <summary>
/// Abstraction over ISyncLocalStorageService from https://github.com/Blazored/LocalStorage
/// that allows persisting game state to given local storage key.
/// </summary>
public class GameStateStorage
{
    private readonly ISyncLocalStorageService _localStorage;

    public GameStateStorage(ISyncLocalStorageService localStorage)
        => _localStorage = localStorage;

    public bool HasGameState 
        => _localStorage.ContainKey(nameof(GameState));

    public JsonObject Read()
        => _localStorage.GetItem<JsonNode>(nameof(GameState)).AsObject();

    public void Persist(GameState gameState)
    {
        Console.Out.WriteLine("Persisting game state");
        _localStorage.SetItem(nameof(GameState), gameState);
    }

    public void Clear()
        => _localStorage.Clear();
}