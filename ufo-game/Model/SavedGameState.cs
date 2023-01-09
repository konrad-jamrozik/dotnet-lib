using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UfoGame.Model;

public static class SavedGameState
{
    public static Factions ReadSaveGameAndRegister(
        PersistentStorage storage,
        IServiceCollection services)
    {
        Console.Out.WriteLine("ReadSaveGame");
        Debug.Assert(storage.HasSavedGame);

        
        JsonObject gameJson = storage.GetItem<JsonNode>(nameof(Game)).AsObject();
        // kja plan of action: manually deserialize all the classes from JsonNode bottom-up, wiring the ctors,
        // then add as singletons.

        var (pendingMissionFaction, factions) = DeserializeFactions(gameJson);
        //new PendingMission(missionPrep, archive, factions, playerScore, staff, money)
        services.AddSingleton(factions);

        Console.Out.WriteLine("Deserialized factions: " + string.Join(" ", factions.Data.Select(f => f.Name)));

        // kja note that even though I build Game singleton from bottom up, just registering
        // Game is not enough, I need to register all dependencies in the entire Game
        // dependency tree. I confirmed this empirically.
        return factions;
    }

    private static (Faction pendingMissionFaction, Factions factions) DeserializeFactions(
        JsonObject gameJson)
    {
        var factionRefMap = new Dictionary<string, Faction?>();
        JsonObject factionsJsonObj = gameJson[nameof(Factions)]!.AsObject();
        JsonObject factionsDataJsonObj = factionsJsonObj[nameof(Factions.Data)]!.AsObject();
        JsonArray factionsDataValuesJsonArray = factionsDataJsonObj["$values"]?.AsArray()!;
        foreach (JsonNode? factionJsonNode in factionsDataValuesJsonArray)
        {
            JsonObject factionJsonObject = factionJsonNode!.AsObject();
            if (factionJsonObject.ContainsKey("$ref"))
            {
                var refId = factionJsonObject["$ref"]!.GetValue<string>();
                if (!factionRefMap.ContainsKey(refId))
                {
                    // This is the first time we encountered a reference to an object,
                    // but we haven't seen the object itself yet, so we add an entry
                    // to the reference map with the object ID and null value.
                    factionRefMap.Add(refId, null);
                }
                else
                {
                    // We encountered a reference to an object, but the reference map
                    // already has the object ID, so this is not the first time we
                    // see a reference to given object, hence nothing to do.
                }
            }
            else
            {
                var id = factionJsonObject["$id"]!.GetValue<string>();
                Faction faction = factionJsonObject.Deserialize<Faction>()!;
                if (!factionRefMap.ContainsKey(id))
                {
                    factionRefMap.Add(id, faction);
                }
                else
                {
                    Debug.Assert(factionRefMap[id] == null);
                    factionRefMap[id] = faction;
                }
            }
        }

        JsonObject pendingMissionJsonObj = gameJson[nameof(PendingMission)]!.AsObject();
        JsonObject pendingMissionFactionJsonObj =
            pendingMissionJsonObj[nameof(PendingMission.Faction)]!.AsObject();

        Faction? pendingMissionFaction = null;
        if (pendingMissionFactionJsonObj.ContainsKey("$ref"))
        {
            var refId = pendingMissionFactionJsonObj["$ref"]!.GetValue<string>();
            pendingMissionFaction = factionRefMap[refId];
        }
        else
        {
            var id = pendingMissionFactionJsonObj["$id"]!.GetValue<string>();
            pendingMissionFaction = pendingMissionFactionJsonObj.Deserialize<Faction>()!;
            factionRefMap[id] = pendingMissionFaction;
        }

        var factions = new Factions(factionRefMap.Values.Select(f => f!).ToList());
        return (pendingMissionFaction!, factions);
    }
}