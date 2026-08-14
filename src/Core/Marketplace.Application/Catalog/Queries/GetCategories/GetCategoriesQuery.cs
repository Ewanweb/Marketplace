using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Marketplace.Application.Catalog.Queries.GetCategories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string NameEn,
    string NamePrs,
    string NamePs,
    string IconName,
    Guid? ParentId,
    int Level);

public sealed record GetCategoriesQuery() : IRequest<Result<List<CategoryDto>>>;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public GetCategoriesQueryHandler(IApplicationDbContext dbContext, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var cacheKey = $"Categories_{culture}";

        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedCategories = System.Text.Json.JsonSerializer.Deserialize<List<CategoryDto>>(cachedData);
            if (cachedCategories != null)
                return Result.Success(cachedCategories);
        }

        var categories = await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto(
                c.Id,
                c.GetName(culture),
                c.NameEn,
                c.NamePrs,
                c.NamePs,
                c.IconName,
                c.ParentId,
                c.Level))
            .ToListAsync(cancellationToken);
            
        var cacheOptions = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
        await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(categories), cacheOptions, cancellationToken);

        return Result.Success(categories);
    }
}
