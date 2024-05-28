using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using UCMaps.Components.Models;
using UCMaps.Components.Models.DTO;

namespace UCMaps
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(HttpClient http, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _http = http;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Get UserSession from secure storage
                string getUserSessionFromStorage = await SecureStorage.Default.GetAsync("UserSession");
                if (string.IsNullOrEmpty(getUserSessionFromStorage))
                    return await Task.FromResult(new AuthenticationState(anonymous));

                // Deserialize into a UserSession object
                var deserializedUserSession = JsonSerializer.Deserialize<UserSession>(getUserSessionFromStorage);
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, deserializedUserSession.Username)
                }, "CustomAuth"));

                // Set token for HttpClient header
                var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deserializedUserSession.Token);

                UCMarker newMarker = new UCMarker
                {
                    Name = "meep",
                    Description = "meep",
                    Lat = 0,
                    Lng = 2
                };

                var response = await httpClient.PostAsJsonAsync("api/Marker", newMarker);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<UCMarker>();
                if (responseData != null)
                {
                    newMarker = responseData;
                }
                return await Task.FromResult(new AuthenticationState(claimsPrincipal));
            }
            catch
            {
                return await Task.FromResult(new AuthenticationState(anonymous));
            }
        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            ClaimsPrincipal claimsPrincipal;
            if (!string.IsNullOrEmpty(userSession.Username))
            {
                string serializeUserSession = JsonSerializer.Serialize(userSession);
                await SecureStorage.Default.SetAsync("UserSession", serializeUserSession);

                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Username)
                }));

                // Set token for HttpClient header
                var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("api");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userSession.Token);
            }
            else
            {
                SecureStorage.Default.Remove("UserSession");
                claimsPrincipal = anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
