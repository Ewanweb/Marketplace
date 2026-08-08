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
    string BannerUrl,
    string BankAccountInfo,
    string KycDetailsJson);

public sealed record GetVendorsQuery(string? Search = null, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedList<VendorDto>>>;

public sealed class GetVendorsQueryHandler : IRequestHandler<GetVendorsQuery, Result<PagedList<VendorDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetVendorsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedList<VendorDto>>> Handle(GetVendorsQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Vendors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v => v.ShopNameEn.ToLower().Contains(search) ||
                                     v.ShopNamePrs.ToLower().Contains(search) ||
                                     v.ShopNamePs.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(v => new VendorDto(
                v.Id,
                v.UserId,
                v.ShopNameEn,
                v.ShopNamePrs,
                v.ShopNamePs,
                v.DescriptionEn,
                v.IsVerified,
                v.Rating,
                v.LogoUrl,
                v.BannerUrl,
                v.BankAccountInfo,
                v.KycDetailsJson))
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<VendorDto>(items, totalCount, request.PageNumber, request.PageSize);

        return Result.Success(pagedList);
    }
}
