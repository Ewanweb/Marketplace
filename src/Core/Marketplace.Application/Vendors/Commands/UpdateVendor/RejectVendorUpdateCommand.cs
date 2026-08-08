using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.UpdateVendor;

public sealed record RejectVendorUpdateCommand(Guid VendorId) : IRequest<Result>;

public sealed class RejectVendorUpdateCommandHandler : IRequestHandler<RejectVendorUpdateCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public RejectVendorUpdateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(RejectVendorUpdateCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor == null)
        {
            return Result.Failure(new Error("NotFound", "Vendor not found."));
        }

        if (!vendor.HasPendingUpdates)
        {
            return Result.Failure(new Error("NoUpdates", "Vendor has no pending updates."));
        }

        vendor.RejectUpdate();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
