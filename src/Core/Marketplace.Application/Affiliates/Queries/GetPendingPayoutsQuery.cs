using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public record PendingPayoutDto(
    Guid UserId,
    string FullName,
    string Email,
    int ReferralCount,
    decimal TotalApprovedAmount);

public sealed record GetPendingPayoutsQuery() : IRequest<Result<List<PendingPayoutDto>>>;

public sealed class GetPendingPayoutsQueryHandler : IRequestHandler<GetPendingPayoutsQuery, Result<List<PendingPayoutDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetPendingPayoutsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<PendingPayoutDto>>> Handle(GetPendingPayoutsQuery request, CancellationToken cancellationToken)
    {
        var approvedReferrals = await _dbContext.AffiliateReferrals
            .Include(r => r.Referrer)
            .Where(r => r.Status == AffiliateStatus.Approved)
            .ToListAsync(cancellationToken);

        var grouped = approvedReferrals
            .GroupBy(r => r.ReferrerUserId)
            .Select(g => new PendingPayoutDto(
                g.Key,
                g.First().Referrer.FullName,
                g.First().Referrer.Email,
                g.Count(),
                g.Sum(r => r.CommissionAmount)))
            .OrderByDescending(p => p.TotalApprovedAmount)
            .ToList();

        return Result.Success(grouped);
    }
}
