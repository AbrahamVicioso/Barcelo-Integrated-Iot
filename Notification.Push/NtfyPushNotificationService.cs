using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Notification.Domain.Entities;
using Notification.Domain.Interfaces;

namespace Notification.Push;

public class NtfyPushNotificationService : IPushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly NtfyOptions _options;
    private readonly ILogger<NtfyPushNotificationService> _logger;

    public NtfyPushNotificationService(
        HttpClient httpClient,
        NtfyOptions options,
        ILogger<NtfyPushNotificationService> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;
    }

    private static string EncodeHeaderValue(string value)
    {
        if (value.All(c => c < 128)) return value;
        return $"=?utf-8?b?{Convert.ToBase64String(Encoding.UTF8.GetBytes(value))}?=";
    }

    public async Task<bool> SendAsync(PushNotification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_options.BaseUrl.TrimEnd('/')}/{notification.Topic}";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(notification.Message, Encoding.UTF8, "text/plain")
            };

            request.Headers.TryAddWithoutValidation("Title", EncodeHeaderValue(notification.Title));
            request.Headers.Add("Priority", ((int)notification.Priority).ToString());

            if (notification.Tags?.Length > 0)
                request.Headers.Add("Tags", string.Join(",", notification.Tags));

            if (!string.IsNullOrEmpty(notification.ClickUrl))
                request.Headers.Add("Click", notification.ClickUrl);

            if (!string.IsNullOrEmpty(_options.AccessToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Push notification sent to topic {Topic}: {Title}", notification.Topic, notification.Title);
                return true;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Push notification failed for topic {Topic}. Status: {Status}. Body: {Body}",
                notification.Topic, response.StatusCode, body);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to topic {Topic}", notification.Topic);
            return false;
        }
    }
}
