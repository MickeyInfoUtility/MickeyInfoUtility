using MickeyInfoUtility;
using MickeyInfoUtility.Interfaces;
using MickeyInfoUtility.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register and configure HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services with their interfaces
builder.Services.AddScoped<IMasterKeyService, MasterKeyService>();
builder.Services.AddScoped<IRenovationService, RenovationService>();
builder.Services.AddScoped<LazyAssemblyLoader>();

// Add configuration
builder.Services.AddScoped(sp =>
    new MasterKeyService(
        sp.GetRequiredService<HttpClient>(),
        sp.GetRequiredService<ILogger<MasterKeyService>>(),
        builder.Configuration
    )
);

// Add logging
builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
