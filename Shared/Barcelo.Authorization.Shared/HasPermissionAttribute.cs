using Microsoft.AspNetCore.Authorization;

namespace Barcelo.Authorization.Shared;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class HasPermissionAttribute : AuthorizeAttribute
{
    public string Permission { get; }

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
        Policy = permission;
    }
}