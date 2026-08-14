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

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null)
        {
            var cleanCode = user.ReferralCode?.Trim().ToLower();
            var unlinkedOrders = await _dbContext.Orders
                .Include(o => o.Items)
                .Where(o => (o.ReferrerUserId == userId || (!string.IsNullOrEmpty(o.ReferralCode) && o.ReferralCode.ToLower() == cleanCode))
                         && o.UserId != userId)
                .ToListAsync(cancellationToken);

            var existingOrderItemIds = await _dbContext.AffiliateReferrals
                .Where(r => r.ReferrerUserId == userId)
                .Select(r => r.OrderItemId)
                .ToListAsync(cancellationToken);

            bool hasChanges = false;
            foreach (var order in unlinkedOrders)
            {
                if (order.ReferrerUserId == null)
                {
                    order.SetReferrer(userId.Value);
                    hasChanges = true;
                }

                foreach (var item in order.Items)
                {
                    if (!existingOrderItemIds.Contains(item.Id))
                    {
                        var refRecord = Domain.Entities.AffiliateReferral.Create(
                            userId.Value,
                            order.Id,
                            item.Id,
                            item.VendorId,
                            item.ProductId,
                            item.TotalPrice,
                            0.05m);
                        refRecord.UpdateStatus(Domain.Entities.AffiliateStatus.Approved);
                        _dbContext.AffiliateReferrals.Add(refRecord);
                        existingOrderItemIds.Add(item.Id);
                        hasChanges = true;
                    }
                }
            }

            if (hasChanges)
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
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
            r.Product?.TitleEn ?? "Purchased Product",
            r.Vendor?.ShopNameEn ?? "Store",
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
