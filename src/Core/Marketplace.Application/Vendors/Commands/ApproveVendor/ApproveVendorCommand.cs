using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.ApproveVendor;

public sealed record ApproveVendorCommand(Guid VendorId, string? Reason = null) : IRequest<Result>;

public sealed class ApproveVendorCommandValidator : AbstractValidator<ApproveVendorCommand>
{
    public ApproveVendorCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class ApproveVendorCommandHandler : IRequestHandler<ApproveVendorCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public ApproveVendorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(ApproveVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _dbContext.Vendors
            .FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);

        if (vendor == null)
        {
            return Result.Failure(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        if (vendor.IsVerified)
        {
            return Result.Failure(Error.Conflict("Vendor.AlreadyVerified", "Vendor is already verified."));
        }

        // 1. Verify the vendor
        vendor.Verify();

        // 2. Add the User as the Owner of the Vendor in VendorMembers
        var isOwnerAlreadyAssigned = await _dbContext.VendorMembers
            .AnyAsync(vm => vm.VendorId == vendor.Id && vm.UserId == vendor.UserId, cancellationToken);

        if (!isOwnerAlreadyAssigned)
        {
            var isUserAlreadyMemberOfAnyVendor = await _dbContext.VendorMembers
                .AnyAsync(vm => vm.UserId == vendor.UserId, cancellationToken);

            if (isUserAlreadyMemberOfAnyVendor)
            {
                return Result.Failure(Error.Conflict("Vendor.UserAlreadyAssigned", Marketplace.Shared.Localization.LocalizedMessage.Get("User is already assigned to another vendor shop.", "این کاربر قبلاً در یک فروشگاه دیگر عضو یا مدیر است.", "دا کارونکی دمخه په بل پلورنځي کې غړی یا مدیر دی.")));
            }

            var vendorMember = VendorMember.Create(vendor.Id, vendor.UserId, VendorRole.Owner);
            _dbContext.VendorMembers.Add(vendorMember);
        }

        // 3. Assign Vendor Role to the user
        var vendorRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Vendor", cancellationToken);
        if (vendorRole != null)
        {
            var userHasVendorRole = await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == vendor.UserId && ur.RoleId == vendorRole.Id, cancellationToken);
            
            if (!userHasVendorRole)
            {
                var userRole = UserRole.Create(vendor.UserId, vendorRole.Id);
                _dbContext.UserRoles.Add(userRole);
            }

            // 4. Ensure Vendor role has all required permissions (Products, Orders, Coupons, Reports)
            var requiredPermissionCodes = new[]
            {
                "Products.Create", "Products.Read", "Products.Update", "Products.Delete",
                "Orders.ViewOwn", "Orders.UpdateStatus",
                "Coupons.Create",
                "Reports.Financial", "Reports.Invoice",
                "Notifications.View"
            };

            var permissions = await _dbContext.Permissions
                .Where(p => requiredPermissionCodes.Contains(p.Code))
                .ToListAsync(cancellationToken);

            var existingRolePermissionIds = await _dbContext.RolePermissions
                .Where(rp => rp.RoleId == vendorRole.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            foreach (var perm in permissions)
            {
                if (!existingRolePermissionIds.Contains(perm.Id))
                {
                    _dbContext.RolePermissions.Add(RolePermission.Create(vendorRole.Id, perm.Id));
                }
            }
        }

        // Notify applicant about approval
        var reasonTextEn = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Reason: {request.Reason}";
        var reasonTextPrs = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" علت: {request.Reason}";
        var reasonTextPs = string.IsNullOrWhiteSpace(request.Reason) ? "" : $" علت: {request.Reason}";

        var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
            "Vendor / Agency Application Approved",
            "تایید درخواست ثبت فروشگاه / نمایندگی",
            "د پلورنځي / نمایندګۍ غوښتنې منل"
        );
        var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
            $"Your vendor application for '{vendor.ShopNameEn}' was approved.{reasonTextEn}",
            $"درخواست ثبت فروشگاه '{vendor.ShopNamePrs}' توسط مدیریت تایید گردید.{reasonTextPrs}",
            $"ستاسو د پلورنځي '{vendor.ShopNamePs}' غوښتنه تایید شوه.{reasonTextPs}"
        );
        var userNotif = Notification.Create(vendor.UserId, notifTitle, notifMsg, NotificationType.SystemAlert);
        _dbContext.Notifications.Add(userNotif);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
