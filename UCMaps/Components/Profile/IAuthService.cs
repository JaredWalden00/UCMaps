using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCMaps.Components.Models;

namespace UCMaps
{

    namespace UCMaps
    {
        public interface IAuthService
        {
            Task<ServiceResponse<string>> Login(string username, string password);
            Task<ServiceResponse<string>> Register(string username, string password);
            Task Logout();
            Task<AuthenticationState> GetAuthenticationStateAsync();
        }
    }


}
