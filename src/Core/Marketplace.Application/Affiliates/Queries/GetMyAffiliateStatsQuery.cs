using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public record AffiliateStatsDto(int TotalReferrals, decimal TotalEarnings, decimal PendingEarnings);

public sealed record GetMyAffiliateStatsQuery() : IRequest<Result<AffiliateStatsDto>>;

public sealed class GetMyAffiliateStatsQueryHandler : IRequestHandler<GetMyAffiliateStatsQuery, Result<AffiliateStatsDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAffiliateStatsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AffiliateStatsDto>> Handle(GetMyAffiliateStatsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
            
        if (userId == null)
        {
            return Result.Failure<AffiliateStatsDto>(Error.Unauthorized("Unauthorized", "User not logged in."));
        }

        var referrals = await _dbContext.AffiliateReferrals
            .Where(r => r.ReferrerUserId == userId)
            .ToListAsync(cancellationToken);

        var totalReferrals = referrals.Count;
        var totalEarnings = referrals.Where(r => r.Status == Domain.Entities.AffiliateStatus.Paid).Sum(r => r.CommissionAmount);
        var pendingEarnings = referrals.Where(r => r.Status == Domain.Entities.AffiliateStatus.Approved || r.Status == Domain.Entities.AffiliateStatus.Pending).Sum(r => r.CommissionAmount);

        return Result.Success(new AffiliateStatsDto(totalReferrals, totalEarnings, pendingEarnings));
    }
}
