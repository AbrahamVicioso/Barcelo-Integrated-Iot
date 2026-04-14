using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Usuarios.Application.Interfaces;
using Usuarios.Domain.Interfaces;
using Usuarios.ExternalService.Repositories;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Lee GrpcUrl primero; si no existe, cae en BaseUrl (compatibilidad hacia atrás)
            var grpcUrl = configuration["ExternalServices:Authentication:GrpcUrl"]
                ?? configuration["ExternalServices:Authentication:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "Missing required configuration 'ExternalServices:Authentication:GrpcUrl'. " +
                    "Add it to appsettings.json or set the environment variable " +
                    "'ExternalServices__Authentication__GrpcUrl'.");

            var skipCertValidation = configuration.GetValue<bool>("ExternalServices:Authentication:SkipCertValidation");

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Canal gRPC → Authentication.Api: {Url}", grpcUrl);

                if (grpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var httpHandler = new HttpClientHandler();
                    if (skipCertValidation)
                        httpHandler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

                    return GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                    {
                        HttpHandler = httpHandler
                    });
                }

                // h2c para desarrollo local (sin TLS)
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

            // Registrar cliente gRPC → Dispositivos.API
            var dispositivosGrpcUrl = configuration["ExternalServices:Dispositivos:GrpcUrl"]
                ?? throw new InvalidOperationException(
                    "Missing required configuration 'ExternalServices:Dispositivos:GrpcUrl'. " +
                    "Add it to appsettings.json or set the environment variable " +
                    "'ExternalServices__Dispositivos__GrpcUrl'.");

            var skipDispositivosCert = configuration.GetValue<bool>("ExternalServices:Dispositivos:SkipCertValidation");

            services.AddSingleton<IDispositivosApiService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DispositivosGrpcClient>>();

                GrpcChannel dispositivosChannel;
                if (dispositivosGrpcUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    var httpHandler = new HttpClientHandler();
                    if (skipDispositivosCert)
                        httpHandler.ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                    dispositivosChannel = GrpcChannel.ForAddress(dispositivosGrpcUrl, new GrpcChannelOptions { HttpHandler = httpHandler });
                }
                else
                {
                    dispositivosChannel = GrpcChannel.ForAddress(dispositivosGrpcUrl, new GrpcChannelOptions
                    {
                        HttpHandler = new SocketsHttpHandler { EnableMultipleHttp2Connections = true }
                    });
                }

                return new DispositivosGrpcClient(dispositivosChannel, logger);
            });

            return services;
        }
    }
}
