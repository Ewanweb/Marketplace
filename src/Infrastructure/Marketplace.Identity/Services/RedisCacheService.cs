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

    public async Task<string> GetProductsCacheVersionAsync(CancellationToken cancellationToken = default)
    {
        var version = await _cache.GetStringAsync("products_cache_version", cancellationToken);
        return string.IsNullOrEmpty(version) ? "1" : version;
    }

    public async Task InvalidateProductsCacheAsync(CancellationToken cancellationToken = default)
    {
        var newVersion = DateTime.UtcNow.Ticks.ToString();
        await _cache.SetStringAsync("products_cache_version", newVersion, cancellationToken);
    }

    public async Task<string> GetCategoriesCacheVersionAsync(CancellationToken cancellationToken = default)
    {
        var version = await _cache.GetStringAsync("categories_cache_version", cancellationToken);
        return string.IsNullOrEmpty(version) ? "1" : version;
    }

    public async Task InvalidateCategoriesCacheAsync(CancellationToken cancellationToken = default)
    {
        var newVersion = DateTime.UtcNow.Ticks.ToString();
        await _cache.SetStringAsync("categories_cache_version", newVersion, cancellationToken);
    }

    public async Task<string> GetBannersCacheVersionAsync(CancellationToken cancellationToken = default)
    {
        var version = await _cache.GetStringAsync("banners_cache_version", cancellationToken);
        return string.IsNullOrEmpty(version) ? "1" : version;
    }

    public async Task InvalidateBannersCacheAsync(CancellationToken cancellationToken = default)
    {
        var newVersion = DateTime.UtcNow.Ticks.ToString();
        await _cache.SetStringAsync("banners_cache_version", newVersion, cancellationToken);
    }
}
