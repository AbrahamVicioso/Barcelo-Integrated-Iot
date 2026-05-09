using System.Collections.Concurrent;

namespace Authentication.Api.Services;

public interface ITwoFactorCacheService
{
    void SetPendingVerification(string userId, string code);
    string? GetPendingVerification(string userId);
    void RemovePendingVerification(string userId);
    bool ValidateAndRemove(string userId, string code);
}

public class TwoFactorCacheService : ITwoFactorCacheService
{
    private readonly ConcurrentDictionary<string, (string Code, DateTime ExpiresAt)> _cache = new();

    public void SetPendingVerification(string userId, string code)
    {
        var expiresAt = DateTime.Now.AddMinutes(5);
        _cache.AddOrUpdate(userId, (code, expiresAt), (_, _) => (code, expiresAt));
    }

    public string? GetPendingVerification(string userId)
    {
        if (_cache.TryGetValue(userId, out var entry))
        {
            if (entry.ExpiresAt > DateTime.Now)
                return entry.Code;
            _cache.TryRemove(userId, out _);
        }
        return null;
    }

    public void RemovePendingVerification(string userId)
    {
        _cache.TryRemove(userId, out _);
    }

    public bool ValidateAndRemove(string userId, string code)
    {
        if (_cache.TryGetValue(userId, out var entry))
        {
            if (entry.ExpiresAt > DateTime.Now && entry.Code == code)
            {
                _cache.TryRemove(userId, out _);
                return true;
            }
        }
        return false;
    }
}