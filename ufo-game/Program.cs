using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame;
using UfoGame.Model;
using UfoGame.ViewModel;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Experimental
builder.Services.AddBlazoredModal();

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorageAsSingleton(config =>
{
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
});
builder.Services.AddSingleton<PersistentStorage>();
AddTypesWithPersistableState(builder);
builder.Services.AddSingleton<Money>();
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<MissionPrep>();
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<MissionLauncher>();
builder.Services.AddSingleton<Game>();
builder.Services.AddSingleton<GameState>();

// ViewModel
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<HireSoldiersPlayerAction>();
builder.Services.AddSingleton<FireSoldiersPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();

void AddTypesWithPersistableState(WebAssemblyHostBuilder builder)
{
    var storage = builder.Build().Services.GetService<PersistentStorage>()!;
    var saveGameReadSuccessfully = false;
    if (storage.HasSavedGame)
    {
        saveGameReadSuccessfully = SavedGameState.TryReadSaveGameAndAddToServices(storage, builder.Services);
    }
    if (!saveGameReadSuccessfully)
    {
        storage.Reset();
        builder.Services.AddSingleton<Timeline>();
        builder.Services.AddSingleton<Factions>();
        builder.Services.AddSingleton<Research>();
        builder.Services.AddSingleton<Archive>();
        builder.Services.AddSingleton(new MoneyData());
        builder.Services.AddSingleton(new PlayerScoreData());
        builder.Services.AddSingleton(new StaffData());
        builder.Services.AddSingleton(new MissionPrepData());
        builder.Services.AddSingleton(new PendingMissionData());
        builder.Services.AddSingleton(new ModalsState());
    }
}
