using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public record AffiliateReferralDto(
    Guid Id, 
    Guid OrderId, 
    string ProductName, 
    string VendorName, 
    decimal OrderItemTotal, 
    decimal CommissionRate, 
    decimal CommissionAmount, 
    string Status, 
    DateTime CreatedAt, 
    DateTime? PaidAt);

public sealed record GetMyReferralsQuery() : IRequest<Result<List<AffiliateReferralDto>>>;

public sealed class GetMyReferralsQueryHandler : IRequestHandler<GetMyReferralsQuery, Result<List<AffiliateReferralDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReferralsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AffiliateReferralDto>>> Handle(GetMyReferralsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
            
        if (userId == null)
        {
            return Result.Failure<List<AffiliateReferralDto>>(Error.Unauthorized("Unauthorized", "User not logged in."));
        }

        var referrals = await _dbContext.AffiliateReferrals
            .Include(r => r.Product)
            .Include(r => r.Vendor)
            .Where(r => r.ReferrerUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        var dtos = referrals.Select(r => new AffiliateReferralDto(
            r.Id,
            r.OrderId,
            r.Product?.TitleEn ?? "Unknown Product",
            r.Vendor?.ShopNameEn ?? "Unknown Vendor",
            r.OrderItemTotal,
            r.CommissionRate,
            r.CommissionAmount,
            r.Status.ToString(),
            r.CreatedAt,
            r.PaidAt
        )).ToList();

        return Result.Success(dtos);
    }
}
