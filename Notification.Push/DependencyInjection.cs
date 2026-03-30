using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Domain.Interfaces;

namespace Notification.Push;

public static class DependencyInjection
{
    public static IServiceCollection AddPushNotificationService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new NtfyOptions();
        configuration.GetSection("Ntfy").Bind(options);
        services.AddSingleton(options);

        services.AddHttpClient<NtfyPushNotificationService>()
            .ConfigurePrimaryHttpMessageHandler(() => BuildHandler(options));
        services.AddSingleton<IPushNotificationService, NtfyPushNotificationService>();

        services.AddHttpClient<NtfyAdminService>()
            .ConfigurePrimaryHttpMessageHandler(() => BuildHandler(options));
        services.AddSingleton<INtfyAdminService, NtfyAdminService>();

        return services;
    }

    private static HttpClientHandler BuildHandler(NtfyOptions options)
    {
        var handler = new HttpClientHandler();
        if (options.IgnoreCertificateErrors)
            handler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        return handler;
    }
}
