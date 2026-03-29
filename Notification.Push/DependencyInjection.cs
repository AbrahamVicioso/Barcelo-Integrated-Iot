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

        services.AddHttpClient<NtfyPushNotificationService>();
        services.AddSingleton<IPushNotificationService, NtfyPushNotificationService>();

        services.AddHttpClient<NtfyAdminService>();
        services.AddSingleton<Notification.Domain.Interfaces.INtfyAdminService, NtfyAdminService>();

        return services;
    }
}
