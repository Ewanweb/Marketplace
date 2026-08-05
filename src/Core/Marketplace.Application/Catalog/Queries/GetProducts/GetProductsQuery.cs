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
    Guid VendorId,
    string VendorName,
    List<string> AvailableSizes,
    List<string> AvailableColors);

public sealed record GetProductsQuery(
    string? SearchQuery = null,
    Guid? CategoryId = null,
    Guid? VendorId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null) : IRequest<Result<List<ProductDto>>>;

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
            .Include(p => p.Vendor)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = $"%{request.SearchQuery.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.TitleEn, search) ||
                EF.Functions.Like(p.TitlePrs, search) ||
                EF.Functions.Like(p.TitlePs, search) ||
                EF.Functions.Like(p.DescriptionEn, search) ||
                EF.Functions.Like(p.DescriptionPrs, search) ||
                EF.Functions.Like(p.DescriptionPs, search));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == request.VendorId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        query = (request.SortBy?.ToLower()) switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "rating_desc" => query.OrderByDescending(p => p.Rating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

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
            p.VendorId,
            p.Vendor != null ? p.Vendor.GetShopName(culture) : "Noorzai Official",
            p.AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
            p.AvailableColors.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList()
        )).ToList();

        return Result.Success(result);
    }
}
