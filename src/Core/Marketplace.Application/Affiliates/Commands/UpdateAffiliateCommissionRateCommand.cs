using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Commands;

public sealed record UpdateAffiliateCommissionRateCommand(Guid VendorId, decimal NewRate) : IRequest<Result>;

public sealed class UpdateAffiliateCommissionRateCommandValidator : AbstractValidator<UpdateAffiliateCommissionRateCommand>
{
    public UpdateAffiliateCommissionRateCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.NewRate).GreaterThanOrEqualTo(0).LessThanOrEqualTo(1);
    }
}

public sealed class UpdateAffiliateCommissionRateCommandHandler : IRequestHandler<UpdateAffiliateCommissionRateCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateAffiliateCommissionRateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateAffiliateCommissionRateCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);
        if (vendor == null)
        {
            return Result.Failure(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        vendor.UpdateAffiliateCommissionRate(request.NewRate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
