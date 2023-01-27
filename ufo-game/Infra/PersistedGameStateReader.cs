using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using UfoGame.Model.Data;

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

            List<Type> persistableTypes = PersistableTypes;
            persistableTypes.ForEach(
                persistableType =>
                {
                    var deserializedInstance = gameJson[persistableType.Name].Deserialize(persistableType)!;
                    services.AddSingletonWithInterfaces(persistableType, deserializedInstance);
                });

            Console.Out.WriteLine(
                "Deserialized all persisted game state and added to service collection. " +
                $"Type instances deserialized & added: {persistableTypes.Count}");
        }
        catch (Exception e)
        {
            Console.Out.WriteLine(
                "Reading persisted game failed! Exception written out to STDERR. " +
                "Clearing persisted and resetting game state.");
            Console.Error.WriteLine(e);
            storage.Clear();
            Reset(services);
        }
    }

    public static void Reset(IServiceCollection services)
    {
        List<Type> persistableTypes = PersistableTypes;
        persistableTypes.ForEach(
            persistableType =>
            {
                var newInstance = Activator.CreateInstance(persistableType)!;
                services.AddSingletonWithInterfaces(persistableType, newInstance);
            });
        Console.Out.WriteLine(
            "Created new instances of all persistable types and added to service collection. " +
            $"Type instances created & added: {persistableTypes.Count}");
    }

    private static List<Type> PersistableTypes
    {
        get
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> persistableTypes = assembly.GetTypes().Where(IsPersistable).ToList();
            return persistableTypes;

            bool IsPersistable(Type type)
                => type.IsAssignableTo(typeof(IPersistable))
                   && type != typeof(IPersistable);
        }
    }
}