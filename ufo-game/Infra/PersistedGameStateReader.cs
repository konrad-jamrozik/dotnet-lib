using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using UfoGame.Model;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public static class PersistedGameStateReader
{
    public static void ReadOrReset(
        GameStateStorage storage,
        IServiceCollection services)
    {
        try
        {
            Console.Out.WriteLine("Reading persisted game state.");
            Debug.Assert(storage.HasGameState);

            JsonObject gameJson = storage.Read();

            var timelineData = gameJson[nameof(TimelineData)].Deserialize<TimelineData>()!;
            var accountingData = gameJson[nameof(AccountingData)].Deserialize<AccountingData>()!;
            var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
            var researchData = gameJson[nameof(ResearchData)].Deserialize<ResearchData>()!;
            var archive = gameJson[nameof(Archive)].Deserialize<Archive>()!;
            var playerScoreData = gameJson[nameof(PlayerScoreData)].Deserialize<PlayerScoreData>()!;
            var missionPrepData = gameJson[nameof(MissionPrepData)].Deserialize<MissionPrepData>()!;
            var pendingMissions = gameJson[nameof(PendingMissions)].Deserialize<PendingMissions>()!;
            var staffData = gameJson[nameof(StaffData)].Deserialize<StaffData>()!;
            var agentsData = gameJson[nameof(AgentsData)].Deserialize<AgentsData>()!;
            var sickBayData = gameJson[nameof(SickBayData)].Deserialize<SickBayData>()!;
            var procurementData = gameJson[nameof(ProcurementData)].Deserialize<ProcurementData>()!;
            var modalsState = gameJson[nameof(ModalsState)].Deserialize<ModalsState>()!;

            // These cannot be rolled into loop, because then I would have to have IEnumerable<object>,
            // and the generic "object" type will prevent the DI framework from recognizing the types.
            services.AddSingleton(timelineData);
            services.AddSingleton(accountingData);
            services.AddSingleton(factions);
            services.AddSingleton(researchData);
            services.AddSingleton(archive);
            services.AddSingleton(playerScoreData);
            services.AddSingleton(staffData);
            services.AddSingleton(agentsData);
            services.AddSingleton(sickBayData);
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
            storage.Clear();
            Reset(services);
        }
    }

    public static void Reset(IServiceCollection services)
    {
        services.AddSingleton<Factions>();
        services.AddSingleton<Archive>();
        services.AddSingleton(new TimelineData());
        services.AddSingleton(new ResearchData());
        services.AddSingleton(new AccountingData());
        services.AddSingleton(new PlayerScoreData());
        services.AddSingleton(new StaffData());
        services.AddSingleton(new AgentsData());
        services.AddSingleton(new SickBayData());
        services.AddSingleton(new MissionPrepData());
        services.AddSingleton(new PendingMissions());
        services.AddSingleton(new ProcurementData());
        services.AddSingleton(new ModalsState());
    }
}