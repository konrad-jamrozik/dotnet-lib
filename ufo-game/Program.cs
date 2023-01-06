using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame;
using UfoGame.Model;
using UfoGame.ViewModel;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorageAsSingleton(config =>
    config.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve
);
builder.Services.AddSingleton<PersistentStorage>();

var storage = builder.Build().Services.GetService<PersistentStorage>()!;

Factions? factions = null;
if (storage.ContainKey(nameof(Game)))
{
    Console.Out.WriteLine("Deserializing Game in Program.cs");
    var game = storage.GetItem<JsonNode>(nameof(Game));
    Console.Out.WriteLine("Deserialized Game in Program.cs");
    Console.Out.WriteLine("game.PendingMission.Faction.Name: " + game?["PendingMission"]?["Faction"]?["Name"]);
    Console.Out.WriteLine("game.Factions.Data: " + game?["Factions"]?["Data"]);
    // kja plan of action: manually deserialize all the classes from JsonNode bottom-up, wiring the ctors,
    // then add as singletons.
    Faction faction = (game?["Factions"]?["Data"]?["$values"]?[0]).Deserialize<Faction>()!;
    Console.Out.WriteLine("game.Factions.Data[0]:" + faction.Name);
    factions = new Factions(new List<Faction> { faction });
}

// Model
builder.Services.AddSingleton<Timeline>();
builder.Services.AddSingleton<Money>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<OperationsArchive>();
builder.Services.AddSingleton<MissionPrep>();
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<StateRefresh>();
if (factions != null)
{
    builder.Services.AddSingleton<Factions>(factions);
}
else
{
    builder.Services.AddSingleton<Factions>();
}
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<Game>();

// ViewModel
builder.Services.AddSingleton<HireSoldiersPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();
