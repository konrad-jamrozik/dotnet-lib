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

// kja can I deserialize Timeline and other classes here, and add to DI?
// Probably PendingMission, that references Faction, would require
// a custom deserializer, to instead reuse existing Faction, based on
// name reference.

// Model
builder.Services.AddSingleton<Timeline>();
builder.Services.AddSingleton<Money>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<OperationsArchive>();
builder.Services.AddSingleton<MissionPrep>();
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<Factions>();
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<PersistentStorage>();
builder.Services.AddSingleton<Game>();

// ViewModel
builder.Services.AddSingleton<HireSoldiersPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();
