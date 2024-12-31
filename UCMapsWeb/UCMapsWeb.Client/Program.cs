using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using UCMapsWeb.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);


string apiBaseAddress;
//apiBaseAddress = "http://ucmapsapi.azurewebsites.net";
apiBaseAddress = "https://localhost:7165"; // Use localhost for other platforms

builder.Services.AddHttpClient("api", httpClient =>
{
    httpClient.BaseAddress = new Uri(apiBaseAddress);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
.CreateClient("api"));
await builder.Build().RunAsync();
