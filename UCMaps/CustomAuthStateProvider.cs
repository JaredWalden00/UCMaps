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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string getUserSessionFromStorage = await SecureStorage.Default.GetAsync("UserSession");
                if (string.IsNullOrEmpty(getUserSessionFromStorage))
                    return new AuthenticationState(anonymous);

                var deserializedUserSession = JsonSerializer.Deserialize<UserSession>(getUserSessionFromStorage);
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, deserializedUserSession.Username),
                new Claim("access_token", deserializedUserSession.Token),
                new Claim(ClaimTypes.NameIdentifier, deserializedUserSession.Id.ToString())
            };
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));

                return new AuthenticationState(claimsPrincipal);
            }
            catch
            {
                return new AuthenticationState(anonymous);
            }
        }

        public async Task UpdateAuthenticationState(UserSession userSession)
        {
            ClaimsPrincipal claimsPrincipal;
            if (!string.IsNullOrEmpty(userSession.Username))
            {
                string serializeUserSession = JsonSerializer.Serialize(userSession);
                await SecureStorage.Default.SetAsync("UserSession", serializeUserSession);

                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userSession.Username),
                new Claim("access_token", userSession.Token),
                new Claim(ClaimTypes.NameIdentifier, userSession.Id.ToString())
            };
                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "CustomAuth"));
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
