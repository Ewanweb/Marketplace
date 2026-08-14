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
        // Auto-sync unlinked referred orders
        var unlinkedOrders = await _dbContext.Orders
            .Include(o => o.Items)
            .Where(o => !string.IsNullOrEmpty(o.ReferralCode) || o.ReferrerUserId != null)
            .ToListAsync(cancellationToken);

        if (unlinkedOrders.Count > 0)
        {
            var users = await _dbContext.Users.ToListAsync(cancellationToken);
            var existingOrderItemIds = await _dbContext.AffiliateReferrals
                .Select(r => r.OrderItemId)
                .ToListAsync(cancellationToken);

            bool hasChanges = false;
            foreach (var order in unlinkedOrders)
            {
                Guid? refId = order.ReferrerUserId;
                if (refId == null && !string.IsNullOrEmpty(order.ReferralCode))
                {
                    var clean = order.ReferralCode.Trim().ToLower();
                    var matchedUser = users.FirstOrDefault(u => u.ReferralCode.Trim().ToLower() == clean && u.Id != order.UserId);
                    if (matchedUser != null)
                    {
                        refId = matchedUser.Id;
                        order.SetReferrer(matchedUser.Id);
                        hasChanges = true;
                    }
                }

                if (refId != null && refId != order.UserId)
                {
                    foreach (var item in order.Items)
                    {
                        if (!existingOrderItemIds.Contains(item.Id))
                        {
                            var refRecord = AffiliateReferral.Create(
                                refId.Value,
                                order.Id,
                                item.Id,
                                item.VendorId,
                                item.ProductId,
                                item.TotalPrice,
                                0.05m);
                            refRecord.UpdateStatus(AffiliateStatus.Approved);
                            _dbContext.AffiliateReferrals.Add(refRecord);
                            existingOrderItemIds.Add(item.Id);
                            hasChanges = true;
                        }
                    }
                }
            }

            if (hasChanges)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        var approvedReferrals = await _dbContext.AffiliateReferrals
            .Include(r => r.Referrer)
            .Where(r => r.Status == AffiliateStatus.Approved || r.Status == AffiliateStatus.Pending)
            .ToListAsync(cancellationToken);

        var grouped = approvedReferrals
            .Where(r => r.Referrer != null)
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
