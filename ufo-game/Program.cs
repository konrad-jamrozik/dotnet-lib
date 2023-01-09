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
{
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
});
builder.Services.AddSingleton<PersistentStorage>();
RegisterClassesWithPersistentState(builder);
builder.Services.AddSingleton<MissionLauncher>();
builder.Services.AddSingleton<Game>();

// ViewModel
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<HireSoldiersPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();

void RegisterClassesWithPersistentState(WebAssemblyHostBuilder builder)
{
    var storage = builder.Build().Services.GetService<PersistentStorage>()!;
    if (storage.HasSavedGame)
    {
        SavedGameState.ReadSaveGameAndAddToServices(storage, builder.Services);
    }
    else
    {
        builder.Services.AddSingleton<Timeline>();
        builder.Services.AddSingleton<Money>();
        builder.Services.AddSingleton<Factions>();
        builder.Services.AddSingleton<Research>();
        builder.Services.AddSingleton<OperationsArchive>();
        builder.Services.AddSingleton<PlayerScore>();
        builder.Services.AddSingleton<Staff>();
        builder.Services.AddSingleton<MissionPrep>();
        builder.Services.AddSingleton<PendingMission>();
    }
}
