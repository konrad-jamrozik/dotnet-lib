using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public static class SavedGameState
{
    public static bool TryReadSaveGameAndAddToServices(
        PersistentStorage storage,
        IServiceCollection services)
    {
        try
        {
            Console.Out.WriteLine("Reading save game");
            Debug.Assert(storage.HasSavedGame);

            JsonObject gameJson = storage.GetItem<JsonNode>(nameof(GameState)).AsObject();

            var timeline = gameJson[nameof(Timeline)].Deserialize<Timeline>()!;
            var accountingData = gameJson[nameof(AccountingData)].Deserialize<AccountingData>()!;
            var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
            var research = gameJson[nameof(Research)].Deserialize<Research>()!;
            var archive = gameJson[nameof(Archive)].Deserialize<Archive>()!;
            var playerScoreData = gameJson[nameof(PlayerScoreData)].Deserialize<PlayerScoreData>()!;
            var missionPrepData = gameJson[nameof(MissionPrepData)].Deserialize<MissionPrepData>()!;
            var pendingMissions = gameJson[nameof(PendingMissions)].Deserialize<PendingMissions>()!;
            var staffData = gameJson[nameof(StaffData)].Deserialize<StaffData>()!;
            var procurementData = gameJson[nameof(ProcurementData)].Deserialize<ProcurementData>()!;
            var modalsState = gameJson[nameof(ModalsState)].Deserialize<ModalsState>()!;

            // These cannot be rolled into loop, because then I would have to have IEnumerable<object>,
            // and the generic "object" type will prevent the DI framework from recognizing the types.
            services.AddSingleton(timeline);
            services.AddSingleton(accountingData);
            services.AddSingleton(factions);
            services.AddSingleton(research);
            services.AddSingleton(archive);
            services.AddSingleton(playerScoreData);
            services.AddSingleton(staffData);
            services.AddSingleton(missionPrepData);
            services.AddSingleton(pendingMissions);
            services.AddSingleton(procurementData);
            services.AddSingleton(modalsState);
            Console.Out.WriteLine("Deserialized all game state and added to service collection.");
            return true;
        }
        catch (Exception e)
        {
            Console.Out.WriteLine("Reading save game failed! Exception:");
            Console.Error.WriteLine(e);
            return false;
        }
    }
}