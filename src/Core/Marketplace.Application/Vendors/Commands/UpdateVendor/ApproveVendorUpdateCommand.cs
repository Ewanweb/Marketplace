using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Marketplace.Application.Vendors.Commands.UpdateVendor;

public sealed record ApproveVendorUpdateCommand(Guid VendorId) : IRequest<Result>;

public sealed class ApproveVendorUpdateCommandHandler : IRequestHandler<ApproveVendorUpdateCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public ApproveVendorUpdateCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ApproveVendorUpdateCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor == null)
        {
            return Result.Failure(new Error("NotFound", "Vendor not found."));
        }

        if (!vendor.HasPendingUpdates || string.IsNullOrEmpty(vendor.PendingUpdatesJson))
        {
            return Result.Failure(new Error("NoUpdates", "Vendor has no pending updates."));
        }

        var updatePayload = JsonSerializer.Deserialize<VendorUpdatePayload>(vendor.PendingUpdatesJson);
        if (updatePayload == null)
        {
            return Result.Failure(new Error("InvalidPayload", "Could not deserialize update payload."));
        }

        vendor.ApproveUpdate(
            updatePayload.ShopNameEn,
            updatePayload.ShopNamePrs,
            updatePayload.ShopNamePs,
            updatePayload.DescriptionEn,
            updatePayload.DescriptionPrs,
            updatePayload.DescriptionPs,
            updatePayload.LogoUrl,
            updatePayload.BannerUrl,
            updatePayload.BankAccountInfo
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class VendorUpdatePayload
{
    public string ShopNameEn { get; set; } = string.Empty;
    public string ShopNamePrs { get; set; } = string.Empty;
    public string ShopNamePs { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public string DescriptionPrs { get; set; } = string.Empty;
    public string DescriptionPs { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    public string BankAccountInfo { get; set; } = string.Empty;
}
