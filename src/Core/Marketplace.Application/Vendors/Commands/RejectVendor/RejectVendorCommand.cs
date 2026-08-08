using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.RejectVendor;

public sealed record RejectVendorCommand(Guid VendorId, string? Reason = null) : IRequest<Result>;

public sealed class RejectVendorCommandValidator : AbstractValidator<RejectVendorCommand>
{
    public RejectVendorCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class RejectVendorCommandHandler : IRequestHandler<RejectVendorCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public RejectVendorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(RejectVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.Vendors
            .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor == null)
        {
            return Result.Failure(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        // Notify applicant about rejection
        var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
            "Vendor / Agency Application Decision",
            "نتیجه درخواست ثبت فروشگاه / نمایندگی",
            "د پلورنځي / نمایندګۍ غوښتنې پرېکړه"
        );
        var reasonTextEn = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Reason: {request.Reason}";
        var reasonTextPrs = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" علت: {request.Reason}";
        var reasonTextPs = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" علت: {request.Reason}";

        var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
            $"Your vendor application for '{vendor.ShopNameEn}' was reviewed and rejected.{reasonTextEn}",
            $"درخواست ثبت فروشگاه '{vendor.ShopNamePrs}' پس از بررسی توسط مدیریت رد گردید.{reasonTextPrs}",
            $"ستاسو د پلورنځي '{vendor.ShopNamePs}' غوښتنه رد شوه.{reasonTextPs}"
        );
        var userNotif = Notification.Create(vendor.UserId, notifTitle, notifMsg, NotificationType.SystemAlert);
        _dbContext.Notifications.Add(userNotif);

        // Remove unverified vendor record
        _dbContext.Vendors.Remove(vendor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
