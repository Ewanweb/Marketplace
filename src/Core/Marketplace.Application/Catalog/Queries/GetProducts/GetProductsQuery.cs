using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Queries.GetProducts;

public sealed record ProductDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    int StockQuantity,
    double Rating,
    string ImageUrl,
    Guid CategoryId,
    List<string> AvailableSizes,
    List<string> AvailableColors);

public sealed record GetProductsQuery(
    string? SearchQuery = null,
    Guid? CategoryId = null) : IRequest<Result<List<ProductDto>>>;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var query = _dbContext.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.Trim().ToLower();
            query = query.Where(p =>
                p.TitleEn.ToLower().Contains(search) ||
                p.TitlePrs.ToLower().Contains(search) ||
                p.TitlePs.ToLower().Contains(search));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        var products = await query.ToListAsync(cancellationToken);

        var result = products.Select(p => new ProductDto(
            p.Id,
            p.GetTitle(culture),
            p.GetDescription(culture),
            p.Price,
            p.StockQuantity,
            p.Rating,
            p.ImageUrl,
            p.CategoryId,
            p.AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
            p.AvailableColors.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList()
        )).ToList();

        return Result.Success(result);
    }
}
