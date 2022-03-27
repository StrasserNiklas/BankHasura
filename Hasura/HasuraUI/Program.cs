using HasuraUI;
using HasuraUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<GraphQlService>(prov => new GraphQlService("://sfr-homework.hasura.app/v1/graphql"));
builder.Services.AddSingleton<BankingService>();

await builder.Build().RunAsync();