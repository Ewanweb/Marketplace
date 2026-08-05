using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

    public GetCategoriesQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

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

        return Result.Success(categories);
    }
}
