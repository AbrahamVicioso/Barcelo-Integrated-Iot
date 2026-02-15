using Dispositivos.Application.DTOs;

namespace Dispositivos.Application.Interfaces;

/// <summary>
/// Interface for Thingsboard device management service abstraction
/// </summary>
public interface ITbDeviceService
{
    /// <summary>
    /// Creates or updates a device in Thingsboard
    /// </summary>
    /// <param name="deviceName">Device name in Thingsboard</param>
    /// <param name="deviceType">Device type</param>
    /// <param name="label">Device label</param>
    /// <param name="accessToken">Optional access token for the device</param>
    /// <param name="nameConflictPolicy">Policy for name conflicts (FAIL or UNIQUIFY)</param>
    /// <param name="uniquifySeparator">Separator for uniquify policy</param>
    /// <param name="uniquifyStrategy">Strategy for uniquify (RANDOM or INCREMENTAL)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Thingsboard device response with ID and credentials</returns>
    Task<TbDeviceResponse> CreateOrUpdateDeviceAsync(
        string deviceName,
        string deviceType,
        string? label = null,
        string? accessToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a device by ID from Thingsboard
    /// </summary>
    /// <param name="deviceId">Thingsboard device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device information</returns>
    Task<TbDeviceResponse?> GetDeviceByIdAsync(
        string deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a device from Thingsboard
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteDeviceAsync(
        string deviceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets device credentials from Thingsboard
    /// </summary>
    /// <param name="deviceId">Device identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Device credentials</returns>
    Task<TbCredentials?> GetDeviceCredentialsAsync(
        string deviceId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Response from Thingsboard device operations
/// </summary>
public class TbDeviceResponse
{
    /// <summary>
    /// Device identifier
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Device name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Device type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Created time (timestamp in milliseconds)
    /// </summary>
    public long? CreatedTime { get; set; }

    /// <summary>
    /// Device credentials
    /// </summary>
    public TbCredentials? Credentials { get; set; }
}

/// <summary>
/// Device credentials from Thingsboard
/// </summary>
public class TbCredentials
{
    /// <summary>
    /// Credentials type (e.g., "ACCESS_TOKEN")
    /// </summary>
    public string? CredentialsType { get; set; }

    /// <summary>
    /// Credentials value (access token)
    /// </summary>
    public string? CredentialsId { get; set; }
}
