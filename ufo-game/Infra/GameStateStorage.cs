using System.Text.Json;
using Blazored.LocalStorage;
using System.Text.Json.Nodes;
using UfoGame.Model.Data;

namespace UfoGame.Infra;

/// <summary>
/// Abstraction over ISyncLocalStorageService from https://github.com/Blazored/LocalStorage
/// that allows persisting game state to given local storage key.
/// </summary>
public class GameStateStorage
{
    private readonly ISyncLocalStorageService _localStorage;

    private readonly JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
    {
        IgnoreReadOnlyProperties = true
    };

    public GameStateStorage(ISyncLocalStorageService localStorage)
        => _localStorage = localStorage;

    public bool HasGameState 
        => _localStorage.ContainKey(nameof(GameState));

    public JsonObject Read()
        => _localStorage.GetItem<JsonNode>(nameof(GameState)).AsObject();

    public void Persist(GameState gameState)
    {
        Console.Out.WriteLine("Persisting game state");
        JsonObject gameStateObject = new JsonObject();
        foreach (IPersistable item in gameState.Persistables)
        {
            // Obtain actual, most-derived runtime type
            // Based on https://stackoverflow.com/a/2520710/986533
            Type itemType = item.GetType();
            // Obtain object representing the data of the runtime type, instead of just the interface.
            // Based on https://stackoverflow.com/a/36067601/986533
            object itemData = Convert.ChangeType(item, itemType);
            // Based on https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/use-dom-utf8jsonreader-utf8jsonwriter?pivots=dotnet-7-0#create-a-jsonnode-dom-with-object-initializers-and-make-changes
            gameStateObject.Add(itemType.Name, JsonSerializer.SerializeToNode(itemData, _serializationOptions)!);

            // Alternative approach, simpler, but ends up having one key per class in local storage,
            // instead of just one key (and hence would require deserialization logic adjustment):
            //_localStorage.SetItem(itemType.Name, itemData);
        }
        _localStorage.SetItem(nameof(GameState), gameStateObject);
    }

    public void Clear()
        => _localStorage.Clear();
}