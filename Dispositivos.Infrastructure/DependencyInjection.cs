using Dispositivos.Application.Interfaces;
using Dispositivos.Infrastructure.Configuration;
using Dispositivos.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dispositivos.Infrastructure;

/// <summary>
/// Dependency injection extensions for Thingsboard infrastructure
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Thingsboard infrastructure services to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddThingsboardInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Thingsboard options
        services.Configure<ThingsboardOptions>(
            configuration.GetSection("Thingsboard"));

        // Register HttpClient for Thingsboard using factory pattern
        services.AddHttpClient<ITbDeviceService, TbDeviceService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ThingsboardOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
