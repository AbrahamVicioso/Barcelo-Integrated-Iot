using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Usuarios.ExternalService.Repositories;
using Usuarios.Domain.Interfaces;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure the gRPC channel
            var authenticationServiceUrl = configuration["AuthenticationService:GrpcUrl"] 
                ?? "http://localhost:5117";

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Creating gRPC channel to: {Url}", authenticationServiceUrl);
                
                return GrpcChannel.ForAddress(authenticationServiceUrl, new GrpcChannelOptions
                {
                    HttpHandler = new SocketsHttpHandler
                    {
                        EnableMultipleHttp2Connections = true
                    }
                });
            });

            services.AddSingleton<AuthenticationGrpcClient>();

            services.AddScoped<IAuthenticationApiClient>(provider =>
            {
                var channel = provider.GetRequiredService<GrpcChannel>();
                var logger = provider.GetRequiredService<ILogger<AuthenticationGrpcClient>>();
                var client = new AuthenticationGrpcClient(channel, logger);
                return client;
            });

            return services;
        }
    }
}
