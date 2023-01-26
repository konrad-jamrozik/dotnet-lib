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
//builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// kja introduce class like new TypeRegistrar(builder).Register();
RegisterPersistenceInfraTypes(builder);
ResetOrReadFromPersistentStorageAndRegisterModelDataTypes(builder);
RegisterModelTypes(builder);
RegisterViewModelTypes(builder);

await builder.Build().RunAsync();

void RegisterPersistenceInfraTypes(WebAssemblyHostBuilder builder)
{
// https://github.com/Blazored/LocalStorage#setup
    builder.Services.AddBlazoredLocalStorageAsSingleton(
        config => { config.JsonSerializerOptions.IgnoreReadOnlyProperties = true; });
    builder.Services.AddSingleton<GameStateStorage>();
    builder.Services.AddSingleton<GameState>();
}

void ResetOrReadFromPersistentStorageAndRegisterModelDataTypes(WebAssemblyHostBuilder builder)
{
    var storage = builder.Build().Services.GetService<GameStateStorage>()!;
    if (storage.HasGameState)
        PersistedGameStateReader.ReadOrReset(storage, builder.Services);
    else
        PersistedGameStateReader.Reset(builder.Services);
}

void RegisterModelTypes(WebAssemblyHostBuilder builder)
{
    List<Type> typesToRegister = new List<Type>
    {
        typeof(Timeline),
        typeof(Accounting),
        typeof(PlayerScore),
        typeof(MissionPrep),
        typeof(Staff),
        typeof(SickBay),
        typeof(Research),
        typeof(Procurement),
        typeof(MissionLauncher),
        typeof(Agents),
        typeof(PendingMission)
    };
    typesToRegister.ForEach(type => builder.Services.AddSingleton(type));
    IServiceProvider serviceProvider = builder.Build().Services;
    typesToRegister.ForEach(type =>
    {
        Console.WriteLine("Activating: " + type);
        serviceProvider.AddSingletonWithInterfaces(builder.Services, type);
    });
}

void RegisterViewModelTypes(WebAssemblyHostBuilder builder)
{
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
}