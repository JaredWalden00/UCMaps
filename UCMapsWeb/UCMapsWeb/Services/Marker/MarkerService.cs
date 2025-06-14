using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using GoogleMapsComponents.Maps;
using UCMapsShared.Models;
using UCMapsShared.Models.DTO;

namespace UCMapsWeb.Services.Marker
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
            var customAuthStateProvider = (CustomAuthStateProvider)_authenticationStateProvider;

            // Await the GetUserIdAsync method to get the user ID
            int? userId = await customAuthStateProvider.GetUserIdAsync();

            if (userId == null)
            {
                // Handle the case where the user ID is not found, e.g., throw an exception or return an error
                throw new UnauthorizedAccessException("User ID not found.");
            }

            var marker = new PostMarkerDto
            {
                Name = newMarker.Name,
                Description = newMarker.Description ?? "",
                Lat = newMarker.Lat,
                Lng = newMarker.Lng,
                StillThereCount = 0,
                NotThereCount = 0,
                UserId = userId.Value // Access the value of the nullable userId
            };

            // Make the HTTP POST request to add the marker, and handle the response accordingly
            var response = await httpClient.PostAsJsonAsync("api/Marker", marker);

            if (response.IsSuccessStatusCode)
            {
                var addedMarker = await response.Content.ReadFromJsonAsync<UCMarker>();
                return addedMarker;
            }
            else
            {
                // Handle error response
                throw new Exception("Failed to add marker.");
            }
        }


        public async Task UpdateMarkerAsync(UCMarker marker)
        {
            var httpClient = _httpClientFactory.CreateClient("api");
            var customAuthStateProvider = (CustomAuthStateProvider)_authenticationStateProvider;
            int? userId = await customAuthStateProvider.GetUserIdAsync();

            if (userId == null)
            {
                // Handle the case where the user ID is not found, e.g., throw an exception or return an error
                throw new UnauthorizedAccessException("User ID not found.");
            }
            var updateMarker = new UpdateMarkerDto
            {
                Id = marker.Id,
                Name = marker.Name,
                Description = marker.Description ?? "",
                Lat = marker.Lat,
                Lng = marker.Lng,
                UserId = userId.Value // Access the value of the nullable userId

            };
                var response = await httpClient.PutAsJsonAsync("api/Marker", updateMarker);
                response.EnsureSuccessStatusCode();
        }
    }
}
