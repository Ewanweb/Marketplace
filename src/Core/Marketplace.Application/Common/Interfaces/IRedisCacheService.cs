namespace Marketplace.Application.Common.Interfaces;

public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task BlacklistTokenAsync(string jti, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<bool> IsTokenBlacklistedAsync(string jti, CancellationToken cancellationToken = default);
}
