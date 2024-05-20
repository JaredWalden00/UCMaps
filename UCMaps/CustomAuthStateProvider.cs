using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UCMaps.Components.Models;
using UCMaps.Components.Models.DTO;
using UCMaps.UCMaps;

namespace UCMaps
{
    public class CustomAuthStateProvider : AuthenticationStateProvider, IAuthService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _configuration;
        private string _token;

        public CustomAuthStateProvider(HttpClient http, IConfiguration configuration)
        {
            _http = http;
            _configuration = configuration;
        }

        public event Func<Task> LoggedIn;
        public event Func<Task> LoggedOut;
        private const string BaseUrl = "http://localhost:5078/Auth/Login";

        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var userLoginDto = new UserLoginDto { Username = username, Password = password };
            var response = await _http.PostAsJsonAsync(BaseUrl, userLoginDto);
            var content = await response.Content.ReadAsStringAsync();
            var serviceResponse = JsonConvert.DeserializeObject<ServiceResponse<string>>(content);

            if (serviceResponse.Success)
            {
                _token = serviceResponse.Data;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<string>> Register(string username, string password)
        {
            var data = new { Username = username, Password = password };
            var response = await _http.PostAsJsonAsync("Auth/Register", data);
            var serviceResponse = new ServiceResponse<string>();

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                serviceResponse.Data = token;
                serviceResponse.Success = true;
                _token = token;
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
            else
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Registration failed. Please check your credentials.";
            }

            return serviceResponse;
        }

        public async Task Logout()
        {
            _token = null;
            _http.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            await LoggedOut?.Invoke();
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new ClaimsIdentity();

            _http.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(_token))
            {
               identity = new ClaimsIdentity(ParseClaimsFromJwt(_token), "jwt"); //parse the token and create a claim identity
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token.Replace("\"", "")); //authentication header is set in the HTTP client for authenticated request 
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state)); //authentication state changed event is called with the information from the user

            return state;
        }

        private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
