using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using UCMapsShared.Models;

namespace UCMapsWeb.Client
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IJSRuntime _jsRuntime;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                // Retrieve token from localStorage
                string userSessionJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "UserSession");

                if (string.IsNullOrEmpty(userSessionJson))
                    return new AuthenticationState(anonymous);

                var userSession = JsonSerializer.Deserialize<UserSession>(userSessionJson);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Username),
                    new Claim("access_token", userSession.Token),
                    new Claim(ClaimTypes.NameIdentifier, userSession.Id.ToString())
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
                // Store the user session in localStorage
                var userSessionJson = JsonSerializer.Serialize(userSession);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "UserSession", userSessionJson);

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
                // Remove the user session from localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "UserSession");
                claimsPrincipal = anonymous;
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }
    }
}
