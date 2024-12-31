using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using GoogleMapsComponents.Maps;
using UCMaps.Blazor_Web_View.Models.DTO;


namespace UCMaps.Services.Marker
{
    public class MarkerService : IMarkerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;

        private const string BaseUrl = "http://localhost:5078/api/Marker";
        private string _userId;

        public MarkerService(IHttpClientFactory httpClientFactory, AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task InitializeUserIdAsync()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity.IsAuthenticated)
            {
                _userId = user.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            }
        }

        public async Task<List<UCMarker>> GetMarkersAsync()
        {
            var httpClient = _httpClientFactory.CreateClient("api");
            return await httpClient.GetFromJsonAsync<List<UCMarker>>("api/Marker");
        }

        public async Task<UCMarker> AddMarkerAsync(UCMarker newMarker)
        {
            var httpClient = _httpClientFactory.CreateClient("api");

            var request = new HttpRequestMessage(HttpMethod.Post, "api/Marker")
            {
                Content = JsonContent.Create(newMarker)
            };

            var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UCMarker>();
        }


        public async Task UpdateMarkerAsync(UCMarker marker)
        {
            var updateMarker = new UpdateMarkerDto
            {
                Id = marker.Id,
                Name = marker.Name,
                Description = marker.Description,
                Lat = marker.Lat,
                Lng = marker.Lng
            };
            var httpClient = _httpClientFactory.CreateClient("api");
                var response = await httpClient.PutAsJsonAsync("api/Marker", updateMarker);
                response.EnsureSuccessStatusCode();
        }
    }
}
