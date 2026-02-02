using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Domain.Interfaces
{
    public interface IAuthenticationApiClient
    {
        Task<Guid> CreateUserAsync(string email, string password);
        Task<Guid?> GetUserIdByEmailAsync(string email);
    }
}
