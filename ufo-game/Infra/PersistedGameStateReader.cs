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

            List<Type> deserializableTypes = DeserializableTypes;
            deserializableTypes.ForEach(
                deserializableType =>
                {
                    var deserializedInstance = gameJson[deserializableType.Name].Deserialize(deserializableType)!;
                    services.AddSingleton(deserializableType, deserializedInstance);
                });

            Console.Out.WriteLine(
                "Deserialized all persisted game state and added to service collection. " +
                $"Type instances deserialized & added: {deserializableTypes.Count}");
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
        List<Type> deserializableTypes = DeserializableTypes;
        deserializableTypes.ForEach(
            deserializableType =>
            {
                var newInstance = Activator.CreateInstance(deserializableType)!;
                services.AddSingleton(deserializableType, newInstance);
            });
        Console.Out.WriteLine(
            "Created new instances of all deserializable types and added to service collection. " +
            $"Type instances created & added: {deserializableTypes.Count}");
    }

    private static List<Type> DeserializableTypes
    {
        get
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<Type> deserializableTypes = assembly.GetTypes().Where(IsDeserializable).ToList();
            return deserializableTypes;

            bool IsDeserializable(Type type)
                => type.IsAssignableTo(typeof(IDeserializable))
                   && type != typeof(IDeserializable);
        }
    }
}