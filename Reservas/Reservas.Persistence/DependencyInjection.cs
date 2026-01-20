using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reservas.Domain.Interfaces;
using Reservas.Persistence.Data;
using Reservas.Persistence.Repositories;

namespace Reservas.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistenceLayer(this IServiceCollection services)
        {
            services.AddDbContext<BarceloReservasContext>((options) =>
            {
                options.UseSqlServer("Name=DefaultConnection");
            });

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Repositories
            services.AddScoped<IReservaRepository, ReservaRepository>();
            services.AddScoped<IActividadRecreativaRepository, ActividadRecreativaRepository>();
            services.AddScoped<IReservaActividadRepository, ReservaActividadRepository>();

            return services;
        }
    }
}
