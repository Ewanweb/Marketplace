using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

using Marketplace.Application.Catalog.Commands.CreateProduct;

namespace Marketplace.Application.Catalog.Queries.GetProducts;

public sealed record ProductDto(
    Guid Id,
    string Title,
    string TitleEn,
    string TitlePrs,
    string TitlePs,
    string Description,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    decimal Price,
    int StockQuantity,
    double Rating,
    string ImageUrl,
    Guid CategoryId,
    Guid VendorId,
    string VendorName,
    List<string> AvailableSizes,
    List<string> AvailableColors,
    List<string> ImageUrls,
    List<ProductAttributeDto> Attributes,
    decimal CustomsFeeAmount = 0);

public sealed record GetProductsQuery(
    string? SearchQuery = null,
    Guid? CategoryId = null,
    Guid? VendorId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? SortBy = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedList<ProductDto>>>;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRedisCacheService _cacheService;

    public GetProductsQueryHandler(IApplicationDbContext dbContext, IRedisCacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var version = await _cacheService.GetProductsCacheVersionAsync(cancellationToken);
        
        var cacheKey = $"Products_{culture}_v{version}_{request.PageNumber}_{request.PageSize}_{request.SearchQuery}_{request.CategoryId}_{request.VendorId}_{request.SortBy}_{request.MinPrice}_{request.MaxPrice}";
        var cachedData = await _cacheService.GetAsync<PagedList<ProductDto>>(cacheKey, cancellationToken);
        if (cachedData != null)
        {
            return Result.Success(cachedData);
        }

        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.Images)
            .Include(p => p.Attributes)
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

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var customsFeeSetting = await _dbContext.SiteSettings.FirstOrDefaultAsync(s => s.Key == "CustomsFeeAmount", cancellationToken);
        decimal customsFee = 0;
        if (customsFeeSetting != null && decimal.TryParse(customsFeeSetting.Value, out var parsedFee))
        {
            customsFee = parsedFee;
        }
        var noorzaiVendorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        var result = products.Select(p => new ProductDto(
            p.Id,
            p.GetTitle(culture),
            p.TitleEn,
            p.TitlePrs,
            p.TitlePs,
            p.GetDescription(culture),
            p.DescriptionEn,
            p.DescriptionPrs,
            p.DescriptionPs,
            p.Price,
            p.StockQuantity,
            p.Rating,
            p.ImageUrl,
            p.CategoryId,
            p.VendorId,
            p.Vendor != null ? p.Vendor.GetShopName(culture) : "Noorzai Official",
            p.AvailableSizes.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList(),
            p.AvailableColors.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList(),
            p.Images != null && p.Images.Count > 0 ? p.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList() : new List<string> { p.ImageUrl },
            p.Attributes != null ? p.Attributes.Select(a => new ProductAttributeDto(a.Key, a.Value)).ToList() : new List<ProductAttributeDto>(),
            p.VendorId == noorzaiVendorId ? customsFee : 0
        )).ToList();

        var pagedList = new PagedList<ProductDto>(result, totalCount, request.PageNumber, request.PageSize);
        await _cacheService.SetAsync(cacheKey, pagedList, TimeSpan.FromMinutes(10), cancellationToken);

        return Result.Success(pagedList);
    }
}
