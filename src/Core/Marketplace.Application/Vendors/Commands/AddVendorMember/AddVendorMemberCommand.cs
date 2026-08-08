using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.AddVendorMember;

public sealed record AddVendorMemberCommand(
    Guid VendorId,
    string UserEmail,
    VendorRole Role = VendorRole.Staff) : IRequest<Result<Guid>>;

public sealed class AddVendorMemberCommandValidator : AbstractValidator<AddVendorMemberCommand>
{
    public AddVendorMemberCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();
    }
}

public sealed class AddVendorMemberCommandHandler : IRequestHandler<AddVendorMemberCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public AddVendorMemberCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(AddVendorMemberCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure<Guid>(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var isOwner = _currentUserService.IsSuperAdmin ||
                      await _dbContext.Vendors.AnyAsync(v => v.Id == request.VendorId && v.UserId == _currentUserService.UserId, cancellationToken) ||
                      await _dbContext.VendorMembers.AnyAsync(vm => vm.VendorId == request.VendorId && vm.UserId == _currentUserService.UserId && vm.Role == VendorRole.Owner, cancellationToken);

        if (!isOwner)
        {
            return Result.Failure<Guid>(Error.Forbidden("Vendor.Forbidden", "Only the vendor owner or admin can add members."));
        }

        var targetUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.UserEmail.Trim().ToLower(), cancellationToken);
        if (targetUser == null)
        {
            return Result.Failure<Guid>(Error.NotFound("User.NotFound", Marketplace.Shared.Localization.LocalizedMessage.Get(
                "User with this email was not found.",
                "کاربری با این ایمیل یافت نشد.",
                "له دې برېښنالیک سره کارونکی ونه موندل شو."
            )));
        }

        var isAlreadyMember = await _dbContext.VendorMembers.AnyAsync(vm => vm.UserId == targetUser.Id, cancellationToken);
        if (isAlreadyMember)
        {
            return Result.Failure<Guid>(Error.Conflict("Vendor.AlreadyMember", Marketplace.Shared.Localization.LocalizedMessage.Get(
                "User is already a member of a vendor shop.",
                "این کاربر قبلاً در یک فروشگاه عضویت دارد.",
                "دا کارونکی دمخه په بل پلورنځي کې غړیتوب لري."
            )));
        }

        var vendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.Id == request.VendorId, cancellationToken);
        var shopName = vendor?.ShopNamePrs ?? vendor?.ShopNameEn ?? "فروشگاه";

        // Create VendorMember in Pending status
        var member = VendorMember.Create(request.VendorId, targetUser.Id, request.Role, VendorMemberStatus.Pending);
        _dbContext.VendorMembers.Add(member);

        // Send Notification to invited user
        var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
            "Store Member Invitation",
            "دعوت‌نامه همکاری در فروشگاه",
            "په پلورنځي کې د همکارۍ غوښتنه"
        );
        var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
            $"You have been invited to join '{shopName}' as a {request.Role}. Please check your invitations to accept or reject.",
            $"شما به عنوان {request.Role} به فروشگاه '{shopName}' دعوت شده‌اید. جهت تایید یا رد دعوت‌نامه به بخش اعلانات مراجعه فرمایید.",
            $"ستاسو د '{shopName}' پلورنځي کې د همکارۍ بلنه درکړل شوې ده."
        );
        var inviteNotif = Notification.Create(targetUser.Id, notifTitle, notifMsg, NotificationType.General);
        _dbContext.Notifications.Add(inviteNotif);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(member.Id);
    }
}
