using Blazored.LocalStorage;
using Darnton.Blazor.DeviceInterop.Geolocation;
using Microsoft.AspNetCore.Components.Authorization;
using UCMapsWeb;
using UCMapsWeb.Components;
using UCMapsWeb.Services.Marker;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();

builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents()
  .AddInteractiveWebAssemblyComponents();
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped<IMarkerService, MarkerService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<IGeolocationService, GeolocationService>();
builder.Services.AddBlazoredLocalStorage();

string apiBaseAddress;
//apiBaseAddress = "http://ucmapsapi.azurewebsites.net";
apiBaseAddress = "https://localhost:7165"; // Use localhost for other platforms

builder.Services.AddHttpClient("api", httpClient =>
{
    httpClient.BaseAddress = new Uri(apiBaseAddress);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
.CreateClient("api"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(UCMapsWeb.Client._Imports).Assembly);

app.Run();
