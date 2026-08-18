using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Marketplace.Application.Catalog.Queries.GetProducts;
using Marketplace.Application.Catalog.Commands.CreateProduct;

namespace Marketplace.Application.Catalog.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _cache;

    public GetProductByIdQueryHandler(IApplicationDbContext dbContext, Microsoft.Extensions.Caching.Distributed.IDistributedCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var cacheKey = $"Product_{request.Id}_{culture}";

        var cachedProduct = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedProduct))
        {
            var productDto = System.Text.Json.JsonSerializer.Deserialize<ProductDto>(cachedProduct);
            if (productDto != null)
                return Result.Success(productDto);
        }

        var p = await _dbContext.Products
            .AsNoTracking()
            .Include(x => x.Vendor)
            .Include(x => x.Images)
            .Include(x => x.Attributes)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (p == null)
        {
            return Result.Failure<ProductDto>(Error.NotFound("Product.NotFound", "The requested product was not found."));
        }

        var customsFeeSetting = await _dbContext.SiteSettings.FirstOrDefaultAsync(s => s.Key == "CustomsFeeAmount", cancellationToken);
        decimal customsFee = 0;
        if (customsFeeSetting != null && decimal.TryParse(customsFeeSetting.Value, out var parsedFee))
        {
            customsFee = parsedFee;
        }
        var noorzaiVendorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

        var result = new ProductDto(
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
        );

        var cacheOptions = new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        
        await _cache.SetStringAsync(cacheKey, System.Text.Json.JsonSerializer.Serialize(result), cacheOptions, cancellationToken);

        return Result.Success(result);
    }
}
