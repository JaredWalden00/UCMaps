using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using UCMapsWeb.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

await builder.Build().RunAsync();
