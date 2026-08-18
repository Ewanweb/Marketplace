using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.RespondVendorInvitation;

public sealed record RespondVendorInvitationCommand(Guid MemberId, bool Accept) : IRequest<Result>;

public sealed class RespondVendorInvitationCommandValidator : AbstractValidator<RespondVendorInvitationCommand>
{
    public RespondVendorInvitationCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public sealed class RespondVendorInvitationCommandHandler : IRequestHandler<RespondVendorInvitationCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RespondVendorInvitationCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RespondVendorInvitationCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var member = await _dbContext.VendorMembers
            .Include(vm => vm.Vendor)
            .Include(vm => vm.User)
            .FirstOrDefaultAsync(vm => vm.Id == request.MemberId && vm.UserId == _currentUserService.UserId, cancellationToken);

        if (member == null)
        {
            return Result.Failure(Error.NotFound("VendorInvitation.NotFound", "Invitation not found."));
        }

        if (member.Status != VendorMemberStatus.Pending)
        {
            return Result.Failure(Error.Conflict("VendorInvitation.AlreadyProcessed", "This invitation has already been processed."));
        }

        var shopName = member.Vendor?.ShopNamePrs ?? member.Vendor?.ShopNameEn ?? "فروشگاه";
        var userFullName = member.User?.FullName ?? "کاربر";

        if (request.Accept)
        {
            member.Accept();

            // Assign proper role according to invitation (Marketer, Staff, or Vendor)
            string targetRoleName = member.Role switch
            {
                VendorRole.Marketer => "Marketer",
                VendorRole.Staff => "Staff",
                VendorRole.Owner => "Vendor",
                _ => "Customer"
            };

            var targetRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == targetRoleName, cancellationToken);
            if (targetRole == null)
            {
                targetRole = Role.Create(targetRoleName, $"{targetRoleName} Role");
                _dbContext.Roles.Add(targetRole);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var userHasRole = await _dbContext.UserRoles.AnyAsync(ur => ur.UserId == member.UserId && ur.RoleId == targetRole.Id, cancellationToken);
            if (!userHasRole)
            {
                _dbContext.UserRoles.Add(UserRole.Create(member.UserId, targetRole.Id));
            }

            // Notify shop owner
            if (member.Vendor != null)
            {
                var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
                    "Store Invitation Accepted",
                    "تایید دعوت‌نامه همکار فروشگاه",
                    "د پلورنځي د همکارۍ بلنه منل شوې"
                );
                var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
                    $"User '{userFullName}' accepted your invitation to join '{shopName}'.",
                    $"کاربر '{userFullName}' دعوت شما را برای عضویت در فروشگاه '{shopName}' پذیرفت.",
                    $"کارونکي '{userFullName}' د '{shopName}' پلورنځي کې ستاسو بلنه ومنله."
                );
                var ownerNotif = Notification.Create(member.Vendor.UserId, notifTitle, notifMsg, NotificationType.General);
                _dbContext.Notifications.Add(ownerNotif);
            }
        }
        else
        {
            _dbContext.VendorMembers.Remove(member);

            // Notify shop owner
            if (member.Vendor != null)
            {
                var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
                    "Store Invitation Rejected",
                    "رد دعوت‌نامه همکار فروشگاه",
                    "د پلورنځي د همکارۍ بلنه رد شوې"
                );
                var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
                    $"User '{userFullName}' rejected your invitation to join '{shopName}'.",
                    $"کاربر '{userFullName}' دعوت شما برای عضویت در فروشگاه '{shopName}' را رد کرد.",
                    $"کارونکي '{userFullName}' د '{shopName}' پلورنځي کې ستاسو بلنه رد کړه."
                );
                var ownerNotif = Notification.Create(member.Vendor.UserId, notifTitle, notifMsg, NotificationType.General);
                _dbContext.Notifications.Add(ownerNotif);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
