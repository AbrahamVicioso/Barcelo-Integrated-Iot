using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Barcelo.Authorization.Shared;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddBarceloAuthorization(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        var allPermissions = Permissions.GetAllPermissions();

        services.AddAuthorizationCore(options =>
        {
            foreach (var permission in allPermissions)
            {
                options.AddPolicy(permission,
                    policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}