using System.Text.Json;
using System.Text.Json.Nodes;

namespace UfoGame.Model;

public class SavedGameState
{
    // kja this should work like that:
    //
    // Overall:
    // if (PersistentStorage.HasSavedGame)
    // {
    //     var savedGame = new SavedGame(PersistentStorage);
    //     savedGame.AddTo(IServiceCollection builder.services);
    // }
    // else
    // {
    //     // register as normal:
    //     builder.services.AddSingleton<Game>(...)
    //     // ...
    // }
    public static Factions? ReadSaveGame(PersistentStorage storage)
    {
        Factions? factions = null;
        if (storage.ContainKey(nameof(Game)))
        {
            Console.Out.WriteLine("Deserializing Game in Program.cs");
            var game = storage.GetItem<JsonNode>(nameof(Game));
            Console.Out.WriteLine("Deserialized Game in Program.cs");
            Console.Out.WriteLine("game.PendingMission.Faction.Name: " + game?["PendingMission"]?["Faction"]?["Name"]);
            Console.Out.WriteLine("game.Factions.Data: " + game?["Factions"]?["Data"]);
            // kja plan of action: manually deserialize all the classes from JsonNode bottom-up, wiring the ctors,
            // then add as singletons.
            // kja note this will fail if the serialization randomized [0] to be the ref entry for PendingMission.
            // In such case I need to first deserialize the ref entry.
            Faction faction = (game?["Factions"]?["Data"]?["$values"]?[0]).Deserialize<Faction>()!;
            Console.Out.WriteLine("game.Factions.Data[0]:" + faction.Name);
            factions = new Factions(new List<Faction> { faction });
        }

        // kja note that even though I build Game singleton from bottom up, just registering
        // Game is not enough, I need to register all dependencies in the entire Game
        // dependency tree. I confirmed this empirically.
        return factions;
    }
}