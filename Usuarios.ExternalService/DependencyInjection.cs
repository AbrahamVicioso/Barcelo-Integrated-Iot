using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Usuarios.ExternalService.Repositories;
using Usuarios.ExternalService.Utils;
using Usuarios.Domain.Interfaces;

namespace Usuarios.ExternalService
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure the gRPC channel
            var grpcUrl = configuration["AuthenticationService:GrpcUrl"]
                ?? "http://localhost:5117";

            // Required for gRPC over plain HTTP (h2c) without TLS
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            services.AddSingleton<GrpcChannel>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<GrpcChannel>>();
                logger.LogInformation("Creating gRPC channel to: {Url}", grpcUrl);

                return GrpcChannel.ForAddress(grpcUrl, new GrpcChannelOptions
                {
                    HttpHandler = new SocketsHttpHandler
                    {
                        EnableMultipleHttp2Connections = true
                    }
                });
            });

            // Configure HttpClient for REST calls to Authentication API
            var authOptions = new AuthenticationApiOptions();
            configuration.GetSection(AuthenticationApiOptions.SectionName).Bind(authOptions);
            var restBaseUrl = authOptions.BaseUrl ?? "http://localhost:5019";

            services.AddHttpClient<AuthenticationGrpcClient>(client =>
            {
                client.BaseAddress = new Uri(restBaseUrl.TrimEnd('/') + "/");
            });

            services.AddScoped<IAuthenticationApiClient>(provider =>
            {
                var channel = provider.GetRequiredService<GrpcChannel>();
                var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(AuthenticationGrpcClient));
                var logger = provider.GetRequiredService<ILogger<AuthenticationGrpcClient>>();
                return new AuthenticationGrpcClient(channel, httpClient, logger);
            });

            return services;
        }
    }
}
