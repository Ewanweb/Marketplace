using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.RegisterVendor;

public sealed record RegisterVendorCommand(
    Guid UserId,
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    string BankAccountInfo,
    string LogoUrl = "",
    string BannerUrl = "",
    string KycDetailsJson = "") : IRequest<Result<Guid>>;

public sealed class RegisterVendorCommandValidator : AbstractValidator<RegisterVendorCommand>
{
    public RegisterVendorCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ShopNameEn).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShopNamePrs).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShopNamePs).NotEmpty().MaximumLength(150);
    }
}

public sealed class RegisterVendorCommandHandler : IRequestHandler<RegisterVendorCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public RegisterVendorCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(RegisterVendorCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (existingUser == null)
        {
            return Result.Failure<Guid>(Error.NotFound("User.NotFound", Marketplace.Shared.Localization.LocalizedMessage.Get("User not found.", "کاربر یافت نشد.", "کارونکی ونه موندل شو.")));
        }

        var existingVendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.UserId == request.UserId, cancellationToken);
        if (existingVendor != null)
        {
            return Result.Failure<Guid>(Error.Conflict("Vendor.AlreadyExists", Marketplace.Shared.Localization.LocalizedMessage.Get("User is already registered as a vendor.", "این کاربر قبلاً به عنوان فروشنده ثبت‌نام کرده است.", "دا کارونکی دمخه د پلورونکي په توګه راجستر شوی دی.")));
        }

        var isAlreadyMember = await _dbContext.VendorMembers.AnyAsync(vm => vm.UserId == request.UserId, cancellationToken);
        if (isAlreadyMember)
        {
            return Result.Failure<Guid>(Error.Conflict("Vendor.AlreadyMember", Marketplace.Shared.Localization.LocalizedMessage.Get("User is already a member of another vendor shop.", "این کاربر قبلاً در یک فروشگاه دیگر عضویت دارد.", "دا کارونکی دمخه په بل پلورنځي کې غړیتوب لري.")));
        }

        var vendor = Vendor.Create(
            request.UserId,
            request.ShopNameEn,
            request.ShopNamePrs,
            request.ShopNamePs,
            request.DescriptionEn,
            request.DescriptionPrs,
            request.DescriptionPs,
            request.LogoUrl,
            request.BannerUrl,
            request.BankAccountInfo,
            request.KycDetailsJson);

        _dbContext.Vendors.Add(vendor);

        // Notify SuperAdmin
        var superAdminId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var notifTitle = Marketplace.Shared.Localization.LocalizedMessage.Get(
            "New Vendor / Agency Application Received",
            "درخواست جدید اخذ نمایندگی / فروشگاه دریافت شد",
            "د نوي نمایندګۍ / پلورنځي غوښتنه ترلاسه شوه"
        );
        var notifMsg = Marketplace.Shared.Localization.LocalizedMessage.Get(
            $"New application submitted for '{request.ShopNameEn}'. Please review in Admin Panel.",
            $"درخواست جدید برای '{request.ShopNamePrs}' ثبت گردید. جهت بررسی و اتخاذ تصمیم به پنل مدیریت مراجعه فرمایید.",
            $"د '{request.ShopNamePs}' لپاره نوې غوښتنه ثبت شوه. مهرباني وکړئ اداره پینل کې بیاکتنه وکړئ."
        );
        var adminNotif = Notification.Create(superAdminId, notifTitle, notifMsg, NotificationType.VendorRegistration);
        _dbContext.Notifications.Add(adminNotif);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(vendor.Id);
    }
}
