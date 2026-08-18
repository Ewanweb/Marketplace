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
    string? ImageUrl,
    Guid? ParentId,
    int Level);

public sealed record GetCategoriesQuery() : IRequest<Result<List<CategoryDto>>>;

public sealed class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRedisCacheService _cacheService;

    public GetCategoriesQueryHandler(IApplicationDbContext dbContext, IRedisCacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var version = await _cacheService.GetCategoriesCacheVersionAsync(cancellationToken);
        var cacheKey = $"Categories_{culture}_v{version}";

        var cachedCategories = await _cacheService.GetAsync<List<CategoryDto>>(cacheKey, cancellationToken);
        if (cachedCategories != null)
        {
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
                c.ImageUrl,
                c.ParentId,
                c.Level))
            .ToListAsync(cancellationToken);
            
        await _cacheService.SetAsync(cacheKey, categories, TimeSpan.FromMinutes(30), cancellationToken);

        return Result.Success(categories);
    }
}
