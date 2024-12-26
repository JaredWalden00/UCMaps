using Auth0.OidcClient;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using UCMaps.Services.Marker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace UCMaps
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });
            
            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddScoped<AuthHeaderHandler>();
            builder.Services.AddScoped<IMarkerService, MarkerService>();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
            builder.Services.AddScoped<CustomAuthStateProvider>();
            builder.Services.AddGeolocationServices();

            string apiBaseAddress;
            apiBaseAddress = "http://ucmapsapi.azurewebsites.net";
#if ANDROID
                        apiBaseAddress = "http://10.0.2.2:5078"; // Android emulator loopback address
#else
            apiBaseAddress = "https://localhost:7165"; // Use localhost for other platforms
#endif

            builder.Services.AddHttpClient("api", httpClient =>
            {
                httpClient.BaseAddress = new Uri(apiBaseAddress);
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
  .CreateClient("api"));

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
