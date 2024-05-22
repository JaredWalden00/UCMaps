using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UCMaps.Components.Models;
using UCMaps.Components.Models.DTO;

namespace UCMaps
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private string _token;
        private ClaimsPrincipal anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        public CustomAuthStateProvider(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public event Func<Task> LoggedIn;
        public event Func<Task> LoggedOut;
        private const string BaseUrl = "http://localhost:5078/Auth/Login";

        //public async Task<ServiceResponse<string>> Login(string username, string password)
        //{
        //    var userLoginDto = new UserLoginDto { Username = username, Password = password };
        //    var response = await _http.PostAsJsonAsync(BaseUrl, userLoginDto);
        //    var content = await response.Content.ReadAsStringAsync();
        //    var serviceResponse = JsonConvert.DeserializeObject<ServiceResponse<string>>(content);

        //    if (serviceResponse.Success)
        //    {
        //        _token = serviceResponse.Data;
        //        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        //    }

        //    return serviceResponse;
        //}
        //public async Task<ServiceResponse<string>> Register(string username, string password)
        //{
        //    var data = new { Username = username, Password = password };
        //    var response = await _http.PostAsJsonAsync("Auth/Register", data);
        //    var serviceResponse = new ServiceResponse<string>();

        //    if (response.IsSuccessStatusCode)
        //    {
        //        var token = await response.Content.ReadAsStringAsync();
        //        serviceResponse.Data = token;
        //        serviceResponse.Success = true;
        //        SetToken(token);
        //        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        //    }
        //    else
        //    {
        //        serviceResponse.Success = false;
        //        serviceResponse.Message = "Registration failed. Please check your credentials.";
        //    }

        //    return serviceResponse;
        //}

        //public async Task Logout()
        //{
        //    _token = null;
        //    _http.DefaultRequestHeaders.Authorization = null;
        //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        //    await LoggedOut?.Invoke();
        //}

        //public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        //{
        //    string getUserSessionFromStorage = await SecureStorage.Default.GetAsync("UserSession");
        //    if (string.IsNullOrEmpty(getUserSessionFromStorage))
        //        return await Task.FromResult(new AuthenticationState(anonymous));

        //    var identity = new ClaimsIdentity();

        //    if (!string.IsNullOrEmpty(_token))
        //    {
        //        identity = new ClaimsIdentity(ParseClaimsFromJwt(_token), "jwt");
        //        SetAuthorizationHeader();
        //    }
        //    else
        //    {
        //        _http.DefaultRequestHeaders.Authorization = null;
        //    }

        //    var user = new ClaimsPrincipal(identity);
        //    var state = new AuthenticationState(user);

        //    NotifyAuthenticationStateChanged(Task.FromResult(state));

        //    return state;
        //}

        //private void SetToken(string token)
        //{
        //    _token = token;          
        //    SetAuthorizationHeader();
        //}

        //private void SetAuthorizationHeader()
        //{
        //    if (!string.IsNullOrEmpty(_token))
        //    {
        //        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        //    }
        //}

        //private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        //{
        //    var payload = jwt.Split('.')[1];
        //    var jsonBytes = ParseBase64WithoutPadding(payload);
        //    var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        //    return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        //}

        //private static byte[] ParseBase64WithoutPadding(string base64)
        //{
        //    switch (base64.Length % 4)
        //    {
        //        case 2: base64 += "=="; break;
        //        case 3: base64 += "="; break;
        //    }
        //    return Convert.FromBase64String(base64);
        //}

        

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                //Get Usersession from secure storage
                string getUserSessionFromStorage = await SecureStorage.Default.GetAsync("UserSession");
                if (string.IsNullOrEmpty(getUserSessionFromStorage))
                    return await Task.FromResult(new AuthenticationState(anonymous));

                //Desrialize into and UserSession object.
                var DesrializedUserSession = System.Text.Json.JsonSerializer.Deserialize<UserSession>(getUserSessionFromStorage);
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, DesrializedUserSession.Username!)
                }, "CustomAuth"));
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
                string serializeUserSession = System.Text.Json.JsonSerializer.Serialize(userSession);
                await SecureStorage.Default.SetAsync("UserSession", serializeUserSession);

                claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userSession.Username!)
                }));
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
