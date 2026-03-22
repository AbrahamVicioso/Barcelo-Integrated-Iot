using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Usuarios.Domain.Interfaces;
using Usuarios.ExternalService.Repositories;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Lee GrpcUrl primero; si no existe, cae en BaseUrl (compatibilidad)
            var grpcUrl = configuration["ExternalServices:Authentication:GrpcUrl"]
                ?? configuration["ExternalServices:Authentication:BaseUrl"]
                ?? "http://localhost:5117";

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Canal gRPC → Authentication.Api: {Url}", grpcUrl);

                return GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = new SocketsHttpHandler
                    {
                        EnableMultipleHttp2Connections = true
                    }
                });
            });

            services.AddScoped<IAuthenticationApiClient>(sp =>
            {
                var channel = sp.GetRequiredService<GrpcChannel>();
                var logger = sp.GetRequiredService<ILogger<AuthenticationGrpcClient>>();
                return new AuthenticationGrpcClient(channel, logger);
            });

            return services;
        }
    }
}
