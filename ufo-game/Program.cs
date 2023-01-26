using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame;
using UfoGame.Infra;
using UfoGame.Model;
using UfoGame.Model.Data;
using UfoGame.ViewModel;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredModal();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

AddGameStateServices(builder);

// kja instead of manually registering for interfaces like IResettable or ITemporal, just pass a list of
// types and use reflection to do all the necessary registration: the type itself, plus any interfaces
// it implements.
// Alternatively, do it like:
// UfoGame.Infra.PersistedGameStateReader.DeserializableTypes
// i.e. get the types from executing assembly, then register. For IDeserializable, IResettable, ITemporal, etc.
// Just need to remember to not register when an instance is already registered.
builder.Services.AddSingleton<Timeline>();
builder.Services.AddSingleton<Accounting>();
builder.Services.AddSingleton<PlayerScore>();
builder.Services.AddSingleton<MissionPrep>();
// kja ongoing experimental work
// List<Type> typesToRegister = new List<Type>
// {
//     typeof(PendingMission), typeof(Staff), typeof(Agents)
// };
// typesToRegister.ForEach(builder.Services.AddSingletonWithInterfaces);
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<IResettable, PendingMission>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<Agents>();
builder.Services.AddSingleton<IResettable, Agents>();
builder.Services.AddSingleton<SickBay>();
builder.Services.AddSingleton<Research>();
builder.Services.AddSingleton<Procurement>();
builder.Services.AddSingleton<MissionLauncher>();

// ViewModel
builder.Services.AddSingleton<ViewStateRefresh>();
builder.Services.AddSingleton<RaiseMoneyPlayerAction>();
builder.Services.AddSingleton<HireAgentsPlayerAction>();
builder.Services.AddSingleton<LaunchMissionPlayerAction>();
builder.Services.AddSingleton<DoNothingPlayerAction>();
builder.Services.AddSingleton<ResearchMoneyRaisingMethodsPlayerAction>();
builder.Services.AddSingleton<ResearchAgentEffectivenessPlayerAction>();
builder.Services.AddSingleton<ResearchAgentSurvivabilityPlayerAction>();
builder.Services.AddSingleton<ResearchTransportCapacityPlayerAction>();
builder.Services.AddSingleton<ResearchAgentRecoverySpeedPlayerAction>();

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
