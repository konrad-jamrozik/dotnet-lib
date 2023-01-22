using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame;
using UfoGame.Infra;
using UfoGame.Model;
using UfoGame.ViewModel;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

AddGameStateServices(builder);

builder.Services.AddSingleton<Accounting>();
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<MissionPrep>();
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<Research>();
builder.Services.AddSingleton<Procurement>();
builder.Services.AddSingleton<MissionLauncher>();
builder.Services.AddSingleton<Game>();

// ViewModel
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<HireAgentsPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();

await builder.Build().RunAsync();

void AddGameStateServices(WebAssemblyHostBuilder builder)
{
    // https://github.com/Blazored/LocalStorage#setup
    builder.Services.AddBlazoredLocalStorageAsSingleton(config =>
    {
        config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    });
    
    builder.Services.AddSingleton<GameStateStorage>();

    var storage = builder.Build().Services.GetService<GameStateStorage>()!;
    if (storage.HasGameState)
        PersistedGameStateReader.ReadOrReset(storage, builder.Services);
    else
        PersistedGameStateReader.Reset(builder.Services);

    builder.Services.AddSingleton<GameState>();
}
