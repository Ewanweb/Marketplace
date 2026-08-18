namespace Marketplace.Application.Common.Interfaces;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task BlacklistTokenAsync(string jti, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default);

    Task<string> GetProductsCacheVersionAsync(CancellationToken cancellationToken = default);
    Task InvalidateProductsCacheAsync(CancellationToken cancellationToken = default);

    Task<string> GetCategoriesCacheVersionAsync(CancellationToken cancellationToken = default);
    Task InvalidateCategoriesCacheAsync(CancellationToken cancellationToken = default);

    Task<string> GetBannersCacheVersionAsync(CancellationToken cancellationToken = default);
    Task InvalidateBannersCacheAsync(CancellationToken cancellationToken = default);
}
