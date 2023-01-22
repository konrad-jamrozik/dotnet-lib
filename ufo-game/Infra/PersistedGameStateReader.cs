using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public static class PersistedGameStateReader
{
    public static void ReadOrResetPersistedGameState(
        GameStateStorage storage,
        IServiceCollection services)
    {
        try
        {
            Console.Out.WriteLine("Reading persisted game state.");
            Debug.Assert(storage.HasGameState);

            JsonObject gameJson = storage.Read();

            var timeline = gameJson[nameof(Timeline)].Deserialize<Timeline>()!;
            var accountingData = gameJson[nameof(AccountingData)].Deserialize<AccountingData>()!;
            var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
            var researchData = gameJson[nameof(ResearchData)].Deserialize<ResearchData>()!;
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
            services.AddSingleton(researchData);
            services.AddSingleton(archive);
            services.AddSingleton(playerScoreData);
            services.AddSingleton(staffData);
            services.AddSingleton(missionPrepData);
            services.AddSingleton(pendingMissions);
            services.AddSingleton(procurementData);
            services.AddSingleton(modalsState);
            Console.Out.WriteLine("Deserialized all game state and added to service collection.");
        }
        catch (Exception e)
        {
            Console.Out.WriteLine("Reading persisted game failed! Exception written out to STDERR. " +
                                  "Clearing persisted and resetting game state.");
            Console.Error.WriteLine(e);
            Reset(storage, services);
        }
    }

    private static void Reset(GameStateStorage storage, IServiceCollection services)
    {
        storage.Clear();
        services.AddSingleton<Timeline>();
        services.AddSingleton<Factions>();
        services.AddSingleton<Archive>();
        services.AddSingleton(new ResearchData());
        services.AddSingleton(new AccountingData());
        services.AddSingleton(new PlayerScoreData());
        services.AddSingleton(new StaffData());
        services.AddSingleton(new MissionPrepData());
        services.AddSingleton(new PendingMissions());
        services.AddSingleton(new ProcurementData());
        services.AddSingleton(new ModalsState());
    }
}