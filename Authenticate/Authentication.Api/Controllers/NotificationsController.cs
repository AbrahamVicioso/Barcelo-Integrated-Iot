using System.Security.Claims;
using Authentication.Api.DTOs;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Notification.Domain.Helpers;
using Notification.Domain.Interfaces;
using Notification.Push;

namespace Authentication.Api.Controllers;

[ApiController]
public class NotificationsController : ControllerBase
{
    private const string NtfyTokenClaim = "ntfy_token";

    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly INtfyAdminService _ntfyAdmin;
    private readonly NtfyOptions _ntfyOptions;

    public NotificationsController(
        UserManager<User> userManager,
        IConfiguration configuration,
        INtfyAdminService ntfyAdmin,
        NtfyOptions ntfyOptions)
    {
        _userManager = userManager;
        _configuration = configuration;
        _ntfyAdmin = ntfyAdmin;
        _ntfyOptions = ntfyOptions;
    }

    /// <summary>
    /// Internal endpoint called by Notification.Worker to persist the ntfy access token.
    /// Not exposed publicly — only reachable within the Docker network.
    /// </summary>
    [HttpPut("/internal/ntfy-token")]
    public async Task<Results<NoContent, NotFound, BadRequest>> StoreNtfyToken(
        [FromBody] StoreNtfyTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.NtfyToken))
            return TypedResults.BadRequest();

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return TypedResults.NotFound();

        var existing = (await _userManager.GetClaimsAsync(user))
            .FirstOrDefault(c => c.Type == NtfyTokenClaim);

        if (existing is not null)
            await _userManager.RemoveClaimAsync(user, existing);

        await _userManager.AddClaimAsync(user, new Claim(NtfyTokenClaim, request.NtfyToken));

        return TypedResults.NoContent();
    }

    /// <summary>
    /// Returns the ntfy push configuration for the authenticated user.
    /// Requires a valid JWT. The client uses the returned token to subscribe in the ntfy app.
    /// </summary>
    [Authorize]
    [HttpGet("/push-config")]
    public async Task<Results<Ok<PushConfigResponse>, NotFound>> GetPushConfig()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return TypedResults.NotFound();

        var claims = await _userManager.GetClaimsAsync(user);
        var ntfyToken = claims.FirstOrDefault(c => c.Type == NtfyTokenClaim)?.Value;

        if (ntfyToken is null)
            return TypedResults.NotFound();

        var ntfyBaseUrl = _ntfyOptions.BaseUrl;
        var topic = NtfyTopicHelper.GetUserTopic(user.Email!);

        // For admins: grant access to the system topic (idempotent) and include it in the response
        string? systemTopic = null;
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            await _ntfyAdmin.GrantSystemTopicAccessAsync(topic, HttpContext.RequestAborted);
            systemTopic = _ntfyOptions.SystemTopic;
        }

        return TypedResults.Ok(new PushConfigResponse(topic, ntfyBaseUrl, ntfyToken, systemTopic));
    }
}

public record StoreNtfyTokenRequest(string Email, string NtfyToken);
