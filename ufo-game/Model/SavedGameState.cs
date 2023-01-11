using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UfoGame.Model;

public static class SavedGameState
{
    public static void ReadSaveGameAndAddToServices(
        PersistentStorage storage,
        IServiceCollection services)
    {
        Console.Out.WriteLine("Reading save game");
        Debug.Assert(storage.HasSavedGame);

        JsonObject gameJson = storage.GetItem<JsonNode>(nameof(Game)).AsObject();

        var timeline = gameJson[nameof(Timeline)].Deserialize<Timeline>()!;
        var money = gameJson[nameof(Money)].Deserialize<Money>()!;
        var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
        var research = gameJson[nameof(Research)].Deserialize<Research>()!;
        var operationsArchive = gameJson[nameof(OperationsArchive)].Deserialize<OperationsArchive>()!;
        var missionPrepData = gameJson[nameof(MissionPrepData)].Deserialize<MissionPrepData>()!;
        var pendingMissionData = gameJson[nameof(PendingMission)]?[nameof(PendingMission.Data)]
            .Deserialize<PendingMissionData>()!;
        // kja all of these field initializations can be avoided, and also chained injections, by doing the following:
        // 1. if a class Foo has a mixture of [JsonInclude] fields, and DI-injected classes,
        // move all [JsonInclude] fields into their own class, FooData.
        // 2. Do here gameJson[nameof(FooData)].Deserialize<FooData>();
        // 3. Register in DI container FooData from save; the logic should initialize new FooData
        // when there was no save. See to-do on UfoGame.Model.PendingMission.Data
        // 4. Register Foo as normal and unconditionally. It will get the injections the usual way.
        //
        // Basically, the idea is that all registrations are done as normal, except
        // these registrations that have data that comes from save file: these classes
        // cannot have any complex classes participating in DI injection, only serialized data.
        var playerScore = new PlayerScore(factions)
        {
            Value = gameJson[nameof(PlayerScore)]![nameof(PlayerScore.Value)]!.GetValue<int>()
        };
        var staff = new Staff(money, playerScore, operationsArchive)
        {
            CurrentSoldiers = gameJson[nameof(Staff)]![nameof(Staff.CurrentSoldiers)]!.GetValue<int>(),
            SoldierEffectiveness = gameJson[nameof(Staff)]![nameof(Staff.SoldierEffectiveness)]!.GetValue<int>(),
            SoldierSurvivability = gameJson[nameof(Staff)]![nameof(Staff.SoldierSurvivability)]!.GetValue<int>(),
            SoldiersToHire = gameJson[nameof(Staff)]![nameof(Staff.SoldiersToHire)]!.GetValue<int>()
        };

        // This cannot be rolled into loop, because then I would have to have IEnumerable<object>,
        // and this "object" will prevent the DI framework from recognizing the types.
        services.AddSingleton(timeline);
        services.AddSingleton(money);
        services.AddSingleton(factions);
        services.AddSingleton(research);
        services.AddSingleton(operationsArchive);
        services.AddSingleton(playerScore);
        services.AddSingleton(staff);
        services.AddSingleton(missionPrepData);
        services.AddSingleton(pendingMissionData);
        Console.Out.WriteLine("Deserialized all game state and added to service collection.");
    }
}