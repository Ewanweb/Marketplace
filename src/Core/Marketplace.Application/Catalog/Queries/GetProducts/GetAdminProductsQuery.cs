using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Queries.GetProducts;

public sealed record GetAdminProductsQuery(
    string? SearchQuery = null,
    Guid? CategoryId = null,
    Guid? VendorId = null,
    string? SortBy = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedList<ProductDto>>>;

public sealed class GetAdminProductsQueryHandler : IRequestHandler<GetAdminProductsQuery, Result<PagedList<ProductDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetAdminProductsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedList<ProductDto>>> Handle(GetAdminProductsQuery request, CancellationToken cancellationToken)
    {
        var culture = CultureInfo.CurrentUICulture.Name;

        var query = _dbContext.Products
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .AsQueryable();

        // Admin scopes
        if (!_currentUserService.IsSuperAdmin)
        {
            var myVendorIds = await _dbContext.VendorMembers
                .Where(vm => vm.UserId == _currentUserService.UserId)
                .Select(vm => vm.VendorId)
                .ToListAsync(cancellationToken);
                
            query = query.Where(p => myVendorIds.Contains(p.VendorId));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = $"%{request.SearchQuery.Trim()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.TitleEn, search) ||
                EF.Functions.Like(p.TitlePrs, search) ||
                EF.Functions.Like(p.TitlePs, search));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.VendorId.HasValue)
        {
            query = query.Where(p => p.VendorId == request.VendorId.Value);
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
            p.Attributes != null ? p.Attributes.Select(a => new Marketplace.Application.Catalog.Commands.CreateProduct.ProductAttributeDto(a.Key, a.Value)).ToList() : new List<Marketplace.Application.Catalog.Commands.CreateProduct.ProductAttributeDto>()
        )).ToList();

        var pagedList = new PagedList<ProductDto>(result, totalCount, request.PageNumber, request.PageSize);

        return Result.Success(pagedList);
    }
}
