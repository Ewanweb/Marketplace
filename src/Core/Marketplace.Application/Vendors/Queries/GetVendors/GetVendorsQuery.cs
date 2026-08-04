using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Queries.GetVendors;

public sealed record VendorDto(
    Guid Id,
    Guid UserId,
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string Description,
    bool IsVerified,
    double Rating,
    string LogoUrl,
    string BannerUrl);

public sealed record GetVendorsQuery(string? Search = null) : IRequest<Result<List<VendorDto>>>;

public sealed class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, Result<List<VendorDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetVendorsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<VendorDto>>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Vendors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v => v.ShopNameEn.ToLower().Contains(search) ||
                                     v.ShopNamePrs.ToLower().Contains(search) ||
                                     v.ShopNamePs.ToLower().Contains(search));
        }

        var vendors = await query.Select(v => new VendorDto(
            v.Id,
            v.UserId,
            v.ShopNameEn,
            v.ShopNamePrs,
            v.ShopNamePs,
            v.DescriptionEn,
            v.IsVerified,
            v.Rating,
            v.LogoUrl,
            v.BannerUrl)).ToListAsync(cancellationToken);

        return Result.Success(vendors);
    }
}
