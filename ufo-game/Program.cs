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

Factions? factions = SavedGameState.ReadSaveGame(storage);

#region Model with persistable state

builder.Services.AddSingleton<Timeline>();
builder.Services.AddSingleton<Money>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<OperationsArchive>();
builder.Services.AddSingleton<MissionPrep>();
if (factions != null)
{
    builder.Services.AddSingleton(factions);
}
else
{
    builder.Services.AddSingleton<Factions>();
}
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<Game>();

#endregion

// ViewModel
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<HireSoldiersPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();
