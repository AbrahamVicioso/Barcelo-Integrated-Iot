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
        services.AddScoped<ICerradurasInteligenteRepository, CerradurasInteligenteRepository>();
        services.AddScoped<ICredencialesAccesoRepository, CredencialesAccesoRepository>();
        services.AddScoped<IMantenimientoCerraduraRepository, MantenimientoCerraduraRepository>();
        services.AddScoped<IRegistrosAccesoRepository, RegistrosAccesoRepository>();
        services.AddScoped<IRegistrosAuditoriumRepository, RegistrosAuditoriumRepository>();

        return services;
    }
}
