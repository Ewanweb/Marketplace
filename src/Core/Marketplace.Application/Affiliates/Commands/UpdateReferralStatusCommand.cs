using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Commands;

public sealed record UpdateReferralStatusCommand(Guid ReferralId, AffiliateStatus NewStatus) : IRequest<Result>;

public sealed class UpdateReferralStatusCommandHandler : IRequestHandler<UpdateReferralStatusCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateReferralStatusCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateReferralStatusCommand request, CancellationToken cancellationToken)
    {
        var referral = await _dbContext.AffiliateReferrals.FirstOrDefaultAsync(r => r.Id == request.ReferralId, cancellationToken);
        if (referral == null)
        {
            return Result.Failure(Error.NotFound("Referral.NotFound", "Referral not found."));
        }

        referral.UpdateStatus(request.NewStatus);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
