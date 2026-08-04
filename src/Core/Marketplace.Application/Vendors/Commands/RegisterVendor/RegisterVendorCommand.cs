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
    string Description,
    string BankAccountInfo,
    string LogoUrl = "",
    string BannerUrl = "") : IRequest<Result<Guid>>;

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
            return Result.Failure<Guid>(Error.NotFound("User.NotFound", "User not found."));
        }

        var existingVendor = await _dbContext.Vendors.FirstOrDefaultAsync(v => v.UserId == request.UserId, cancellationToken);
        if (existingVendor != null)
        {
            return Result.Failure<Guid>(Error.Conflict("Vendor.AlreadyExists", "User is already registered as a vendor."));
        }

        var vendor = Vendor.Create(
            request.UserId,
            request.ShopNameEn,
            request.ShopNamePrs,
            request.ShopNamePs,
            request.Description,
            request.Description,
            request.Description,
            request.LogoUrl,
            request.BannerUrl);

        _dbContext.Vendors.Add(vendor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(vendor.Id);
    }
}
