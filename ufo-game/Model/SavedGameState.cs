﻿using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UfoGame.Model;

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
            var moneyData = gameJson[nameof(Money)]?[nameof(Money.Data)].Deserialize<MoneyData>()!;
            var factions = gameJson[nameof(Factions)].Deserialize<Factions>()!;
            var research = gameJson[nameof(Research)].Deserialize<Research>()!;
            var operationsArchive = gameJson[nameof(Archive)].Deserialize<Archive>()!;
            var playerScoreData = gameJson[nameof(PlayerScore)]?[nameof(PlayerScore.Data)].Deserialize<PlayerScoreData>()!;
            var missionPrepData = gameJson[nameof(MissionPrep)]?[nameof(MissionPrep.Data)].Deserialize<MissionPrepData>()!;
            var pendingMissionData = gameJson[nameof(PendingMission)]?[nameof(PendingMission.Data)]
                .Deserialize<PendingMissionData>()!;
            var staffData = gameJson[nameof(Staff)]?[nameof(Staff.Data)].Deserialize<StaffData>()!;
            var modalsState = gameJson[nameof(ModalsState)].Deserialize<ModalsState>()!;

            // These cannot be rolled into loop, because then I would have to have IEnumerable<object>,
            // and the generic "object" type will prevent the DI framework from recognizing the types.
            services.AddSingleton(timeline);
            services.AddSingleton(moneyData);
            services.AddSingleton(factions);
            services.AddSingleton(research);
            services.AddSingleton(operationsArchive);
            services.AddSingleton(playerScoreData);
            services.AddSingleton(staffData);
            services.AddSingleton(missionPrepData);
            services.AddSingleton(pendingMissionData);
            services.AddSingleton(modalsState);
            Console.Out.WriteLine("Deserialized all game state and added to service collection.");
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}