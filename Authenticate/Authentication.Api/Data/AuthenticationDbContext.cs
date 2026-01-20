using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Api.Data
{
    public class AuthenticationDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
            :base(options)
        {
            
        }
    }
}
