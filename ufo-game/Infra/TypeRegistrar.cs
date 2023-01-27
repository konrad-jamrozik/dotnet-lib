using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame.Model;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public class TypeRegistrar
{
    public void RegisterTypes(WebAssemblyHostBuilder builder)
    {
        RegisterMiscInfraTypes(builder);
        RegisterPersistenceInfraTypes(builder);
        ResetOrReadFromPersistentStorageAndRegisterModelDataTypes(builder);
        RegisterModelTypes(builder);
        RegisterViewModelTypes(builder);
    }

    private static void RegisterMiscInfraTypes(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddBlazoredModal();
        //builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    }

    static void RegisterPersistenceInfraTypes(WebAssemblyHostBuilder builder)
    {
        // https://github.com/Blazored/LocalStorage#setup
        builder.Services.AddBlazoredLocalStorageAsSingleton(
            config => { config.JsonSerializerOptions.IgnoreReadOnlyProperties = true; });
        builder.Services.AddSingleton<GameStateStorage>();
        builder.Services.AddSingleton<GameState>();
    }

    static void ResetOrReadFromPersistentStorageAndRegisterModelDataTypes(WebAssemblyHostBuilder builder)
    {
        var storage = builder.Build().Services.GetService<GameStateStorage>()!;
        if (storage.HasGameState)
            PersistedGameStateReader.ReadOrReset(storage, builder.Services);
        else
            PersistedGameStateReader.Reset(builder.Services);
    }

    static void RegisterModelTypes(WebAssemblyHostBuilder builder)
    {
        List<Type> typesToRegister = new List<Type>
        {
            typeof(Timeline),
            typeof(Accounting),
            typeof(PlayerScore),
            typeof(MissionDeployment),
            typeof(SickBay),
            typeof(Research),
            typeof(Procurement),
            typeof(MissionLauncher),
            typeof(Agents),
            typeof(MissionSite)
        };
        typesToRegister.ForEach(type => builder.Services.AddSingleton(type));
        IServiceProvider serviceProvider = builder.Build().Services;
        typesToRegister.ForEach(type => { serviceProvider.AddSingletonWithInterfaces(builder.Services, type); });
    }

    static void RegisterViewModelTypes(WebAssemblyHostBuilder builder)
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
}