using Blazored.LocalStorage;
using Blazored.Modal;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame.Model;
using UfoGame.ViewModel;

namespace UfoGame.Infra;

public static class TypeRegistrar
{
    public static void RegisterTypes(WebAssemblyHostBuilder builder)
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
        builder.Services.AddSingleton<RandomGen>();
        //builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    }

    private static void RegisterPersistenceInfraTypes(WebAssemblyHostBuilder builder)
    {
        // https://github.com/Blazored/LocalStorage#setup
        builder.Services.AddBlazoredLocalStorageAsSingleton(
            config => { config.JsonSerializerOptions.IgnoreReadOnlyProperties = true; });
        builder.Services.AddSingleton<GameStateStorage>();
        builder.Services.AddSingleton<GameState>();
    }

    private static void ResetOrReadFromPersistentStorageAndRegisterModelDataTypes(WebAssemblyHostBuilder builder)
    {
        var storage = builder.Build().Services.GetService<GameStateStorage>()!;
        if (storage.HasGameState)
            PersistedGameStateReader.ReadOrReset(storage, builder.Services);
        else
            PersistedGameStateReader.Reset(builder.Services);
    }

    /// <summary>
    /// Register non-static classes in namespace UfoGame.Model.
    /// All non-static classes in UfoGame.Model namespace SHOULD NOT implement IPersistable interface.
    ///
    /// Types implementing IPersistable interface should:
    /// - be in UfoGame.Model.Data namespace;
    /// - have their name end with "Data".
    /// 
    /// Alternatively, such types should:
    /// - be in UfoGame.ViewModel namespace;
    /// - have their name end with "State".
    ///
    /// All such types are registered in the
    ///
    ///   UfoGame.Infra.PersistedGameStateReader.ReadOrReset
    /// 
    /// method.
    /// </summary>
    private static void RegisterModelTypes(WebAssemblyHostBuilder builder)
    {
        var typesToRegister = new List<Type>
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
            typeof(MissionSite),
            typeof(MissionStats),
            typeof(MissionOutcome)
        };
        typesToRegister.ForEach(type => builder.Services.AddSingleton(type));
        IServiceProvider serviceProvider = builder.Build().Services;
        typesToRegister.ForEach(type => { serviceProvider.AddSingletonWithInterfaces(builder.Services, type); });
    }

    private static void RegisterViewModelTypes(WebAssemblyHostBuilder builder)
    {
        var typesToRegister = new List<Type>
        {
            typeof(ViewStateRefresh),
            typeof(RaiseMoneyPlayerAction),
            typeof(HireAgentsPlayerAction),
            typeof(LaunchMissionPlayerAction),
            typeof(DoNothingPlayerAction),
            typeof(ResearchMoneyRaisingMethodsPlayerAction),
            typeof(ResearchAgentEffectivenessPlayerAction),
            typeof(ResearchAgentSurvivabilityPlayerAction),
            typeof(ResearchTransportCapacityPlayerAction),
            typeof(ResearchAgentRecoverySpeedPlayerAction)
        };
        typesToRegister.ForEach(type => builder.Services.AddSingleton(type));
    }
}