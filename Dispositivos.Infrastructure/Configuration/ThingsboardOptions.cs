namespace Dispositivos.Infrastructure.Configuration;

/// <summary>
/// Configuration options for Thingsboard API
/// </summary>
public class ThingsboardOptions
{
    /// <summary>
    /// Thingsboard API base URL
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Thingsboard username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Thingsboard password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Tenant administrator username
    /// </summary>
    public string TenantUsername { get; set; } = string.Empty;

    /// <summary>
    /// Tenant administrator password
    /// </summary>
    public string TenantPassword { get; set; } = string.Empty;

    /// <summary>
    /// Default name conflict policy (FAIL or UNIQUIFY)
    /// </summary>
    public string NameConflictPolicy { get; set; } = "FAIL";

    /// <summary>
    /// Default uniquify separator
    /// </summary>
    public string UniquifySeparator { get; set; } = "_";

    /// <summary>
    /// Default uniquify strategy (RANDOM or INCREMENTAL)
    /// </summary>
    public string UniquifyStrategy { get; set; } = "RANDOM";

    /// <summary>
    /// JWT token expiration in minutes
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 60;
}
