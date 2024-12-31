using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace UCMapsWeb.Client
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AuthenticationStateProvider _authStateProvider;

        public AuthHeaderHandler(AuthenticationStateProvider authStateProvider)
        {
            _authStateProvider = authStateProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity.IsAuthenticated)
            {
                var token = user.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            var response = await base.SendAsync(request, cancellationToken);
            return response;
        }
    }



}
