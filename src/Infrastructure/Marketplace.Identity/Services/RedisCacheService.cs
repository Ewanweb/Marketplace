using System.Text.Json;
using Marketplace.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Marketplace.Identity.Services;

public sealed class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var data = await _cache.GetStringAsync(key, cancellationToken);
        if (string.IsNullOrEmpty(data))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions();
        if (expiry.HasValue)
        {
            options.SetAbsoluteExpiration(expiry.Value);
        }

        var payload = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, payload, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task BlacklistTokenAsync(string jti, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var key = $"blacklist:{jti}";
        await SetAsync(key, "revoked", expiry, cancellationToken);
    }

    public async Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default)
    {
        var key = $"blacklist:{jti}";
        var result = await GetAsync<string>(key, cancellationToken);
        return !string.IsNullOrEmpty(result);
    }
}
