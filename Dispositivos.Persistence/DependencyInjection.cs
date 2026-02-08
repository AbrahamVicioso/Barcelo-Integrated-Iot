using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Dispositivos.Application.Interfaces;
using Dispositivos.Persistence.Data;
using Dispositivos.Persistence.Repositories;

namespace Dispositivos.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, string connectionString)
    {
        // Register DbContext
        services.AddDbContext<BarceloIoTDatabaseContext>(options =>
            options.UseSqlServer(connectionString));

        // Register UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Repositories
        services.AddScoped<IDispositivoRepository, DispositivoRepository>();

        return services;
    }
}
