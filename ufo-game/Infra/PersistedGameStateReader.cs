using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
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

            Assembly assembly = Assembly.GetExecutingAssembly();

            List<Type> persistableTypes = assembly.GetTypes().Where(ImplementsIData).ToList();
            foreach (Type persistableType in persistableTypes)
            {
                var persistedData = gameJson[persistableType.Name].Deserialize(persistableType)!;
                services.AddSingleton(persistableType, persistedData);
            }

            Console.Out.WriteLine(
                "Deserialized all persisted game state and added to service collection. " +
                $"Persistable data types added: {persistableTypes.Count}");
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
        services.AddSingleton<FactionsData>();
        services.AddSingleton<ArchiveData>();
        services.AddSingleton(new TimelineData());
        services.AddSingleton(new ResearchData());
        services.AddSingleton(new AccountingData());
        services.AddSingleton(new PlayerScoreData());
        services.AddSingleton(new StaffData());
        services.AddSingleton(new AgentsData());
        services.AddSingleton(new SickBayData());
        services.AddSingleton(new MissionPrepData());
        services.AddSingleton(new PendingMissionsData());
        services.AddSingleton(new ProcurementData());
        services.AddSingleton(new ModalsState());
    }

    private static bool ImplementsIData(Type type)
    {
        return type.IsAssignableTo(typeof(IData))
               && type != typeof(IData)
               // kja replace with custom attribute
               && type != typeof(AgentData)
               && type != typeof(FactionData)
               && type != typeof(PendingMissionData);
    }
}