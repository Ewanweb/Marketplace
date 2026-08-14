using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Queries;

public record VendorDetailDto(
    Guid Id,
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    string LogoUrl,
    string BannerUrl,
    string BankAccountInfo,
    string KycDetailsJson,
    bool IsVerified,
    bool IsActive,
    bool HasPendingUpdates,
    string PendingUpdatesJson,
    decimal CommissionRate,
    decimal AffiliateCommissionRate);

public sealed record GetMyVendorsQuery() : IRequest<Result<List<VendorDetailDto>>>;

public sealed class GetMyVendorsQueryHandler : IRequestHandler<GetMyVendorsQuery, Result<List<VendorDetailDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyVendorsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<VendorDetailDto>>> Handle(GetMyVendorsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure<List<VendorDetailDto>>(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var query = _dbContext.Vendors.AsNoTracking().Where(v => v.IsActive);

        if (!_currentUserService.IsSuperAdmin)
        {
            var myVendorIds = await _dbContext.VendorMembers
                .Where(vm => vm.UserId == _currentUserService.UserId && vm.Status == VendorMemberStatus.Accepted)
                .Select(vm => vm.VendorId)
                .ToListAsync(cancellationToken);
                
            query = query.Where(v => v.UserId == _currentUserService.UserId || myVendorIds.Contains(v.Id));
        }

        var vendors = await query
            .OrderBy(v => v.ShopNameEn)
            .Select(v => new VendorDetailDto(
                v.Id,
                v.ShopNameEn,
                v.ShopNamePrs,
                v.ShopNamePs,
                v.DescriptionEn,
                v.DescriptionPrs,
                v.DescriptionPs,
                v.LogoUrl,
                v.BannerUrl,
                v.BankAccountInfo,
                v.KycDetailsJson,
                v.IsVerified,
                v.IsActive,
                v.HasPendingUpdates,
                v.PendingUpdatesJson,
                v.CommissionRate,
                v.AffiliateCommissionRate
            ))
            .ToListAsync(cancellationToken);

        return Result.Success(vendors);
    }
}
