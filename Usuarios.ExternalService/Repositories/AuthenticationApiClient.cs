using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Usuarios.Domain.Interfaces;

namespace Usuarios.ExternalService.Repositories
{
    public class AuthenticationApiClient : IAuthenticationApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthenticationApiClient(HttpClient httpClient)
        {
            this._httpClient = httpClient;
        }

        public async Task<Guid> CreateUserAsynx(string email, string password)
        {
            var response = _httpClient.PostAsJsonAsync("/api/auth/register", 
                new { Email = email, Password = password }
            ).Result;

            response.EnsureSuccessStatusCode();

            return Guid.NewGuid();
        }
    }
}
