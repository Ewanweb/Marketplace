using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Commands;

public sealed record ProcessAffiliatePayoutCommand(Guid AffiliateUserId) : IRequest<Result<decimal>>;

public sealed class ProcessAffiliatePayoutCommandHandler : IRequestHandler<ProcessAffiliatePayoutCommand, Result<decimal>>
{
    private readonly IApplicationDbContext _dbContext;

    public ProcessAffiliatePayoutCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<decimal>> Handle(ProcessAffiliatePayoutCommand request, CancellationToken cancellationToken)
    {
        var referrals = await _dbContext.AffiliateReferrals
            .Where(r => r.ReferrerUserId == request.AffiliateUserId && r.Status == AffiliateStatus.Approved)
            .ToListAsync(cancellationToken);

        if (referrals.Count == 0)
        {
            return Result.Failure<decimal>(Error.Validation("Payout.NoApprovedReferrals",
                "No approved referrals found for this user to pay out."));
        }

        var totalAmount = referrals.Sum(r => r.CommissionAmount);

        foreach (var referral in referrals)
        {
            referral.UpdateStatus(AffiliateStatus.Paid);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(totalAmount);
    }
}
