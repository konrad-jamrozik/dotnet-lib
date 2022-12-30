using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UfoGame;
using UfoGame.Model;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Game State
builder.Services.AddSingleton<Timeline>();
builder.Services.AddSingleton<Money>();
builder.Services.AddSingleton<Staff>();
builder.Services.AddSingleton<OperationsArchive>();
builder.Services.AddSingleton<MissionPrep>();
builder.Services.AddSingleton<PendingMission>();
builder.Services.AddSingleton<StateRefresh>();
builder.Services.AddSingleton<Factions>();
builder.Services.AddSingleton<Game>();

await builder.Build().RunAsync();
