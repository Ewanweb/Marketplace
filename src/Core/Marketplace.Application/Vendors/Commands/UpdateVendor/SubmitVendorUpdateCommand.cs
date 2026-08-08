using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Marketplace.Application.Vendors.Commands.UpdateVendor;

public sealed record SubmitVendorUpdateCommand(
    Guid VendorId,
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    string LogoUrl,
    string BannerUrl,
    string BankAccountInfo) : IRequest<Result>;

public sealed class SubmitVendorUpdateCommandValidator : AbstractValidator<SubmitVendorUpdateCommand>
{
    public SubmitVendorUpdateCommandValidator()
    {
        RuleFor(v => v.VendorId).NotEmpty();
        RuleFor(v => v.ShopNameEn).NotEmpty().MaximumLength(150);
    }
}

public sealed class SubmitVendorUpdateCommandHandler : IRequestHandler<SubmitVendorUpdateCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public SubmitVendorUpdateCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SubmitVendorUpdateCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure(new Error("Unauthorized", "User is not authorized."));
        }

        var userId = _currentUserService.UserId.Value;

        var vendor = await _dbContext.Vendors
            .FirstOrDefaultAsync(v => v.Id == request.VendorId && v.UserId == userId, cancellationToken);

        if (vendor == null)
        {
            return Result.Failure(new Error("NotFound", "Vendor not found or you do not have permission."));
        }

        var updatePayload = new
        {
            request.ShopNameEn,
            request.ShopNamePrs,
            request.ShopNamePs,
            request.DescriptionEn,
            request.DescriptionPrs,
            request.DescriptionPs,
            request.LogoUrl,
            request.BannerUrl,
            request.BankAccountInfo
        };

        var json = JsonSerializer.Serialize(updatePayload);
        vendor.SubmitUpdate(json);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
