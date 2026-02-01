using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Usuarios.Domain.Interfaces
{
    public interface IAuthenticationApiClient
    {
        Task<Guid> CreateUserAsynx(string email, string password);
    }
}
