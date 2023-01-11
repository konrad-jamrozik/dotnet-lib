﻿using System.Diagnostics;
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

        JsonObject gameJson = storage.GetItem<JsonNode>(nameof(GameState)).AsObject();

        var timeline = gameJson[nameof(Timeline)].Deserialize<Timeline>()!;
        var money = gameJson[nameof(Money)].Deserialize<Money>()!;
        var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
        var research = gameJson[nameof(Research)].Deserialize<Research>()!;
        var operationsArchive = gameJson[nameof(OperationsArchive)].Deserialize<OperationsArchive>()!;
        var playerScoreData = gameJson[nameof(PlayerScore)]?[nameof(PlayerScore.Data)].Deserialize<PlayerScoreData>()!;
        var missionPrepData = gameJson[nameof(MissionPrep)]?[nameof(MissionPrep.Data)].Deserialize<MissionPrepData>()!;
        var pendingMissionData = gameJson[nameof(PendingMission)]?[nameof(PendingMission.Data)]
            .Deserialize<PendingMissionData>()!;
        var staffData = gameJson[nameof(Staff)]?[nameof(Staff.Data)].Deserialize<StaffData>()!;

        // These cannot be rolled into loop, because then I would have to have IEnumerable<object>,
        // and the generic "object" type will prevent the DI framework from recognizing the types.
        services.AddSingleton(timeline);
        services.AddSingleton(money);
        services.AddSingleton(factions);
        services.AddSingleton(research);
        services.AddSingleton(operationsArchive);
        services.AddSingleton(playerScoreData);
        services.AddSingleton(staffData);
        services.AddSingleton(missionPrepData);
        services.AddSingleton(pendingMissionData);
        Console.Out.WriteLine("Deserialized all game state and added to service collection.");
    }
}