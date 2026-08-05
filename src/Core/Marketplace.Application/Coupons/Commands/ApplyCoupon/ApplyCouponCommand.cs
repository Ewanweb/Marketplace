using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Coupons.Commands.ApplyCoupon;

public sealed record ApplyCouponResultDto(
    string Code,
    decimal OriginalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    bool IsPercentage,
    decimal DiscountPercent);

public sealed record ApplyCouponCommand(
    string Code,
    decimal OrderAmount) : IRequest<Result<ApplyCouponResultDto>>;

public sealed class ApplyCouponCommandValidator : AbstractValidator<ApplyCouponCommand>
{
    public ApplyCouponCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OrderAmount).GreaterThan(0);
    }
}

public sealed class ApplyCouponCommandHandler : IRequestHandler<ApplyCouponCommand, Result<ApplyCouponResultDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public ApplyCouponCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ApplyCouponResultDto>> Handle(ApplyCouponCommand request, CancellationToken cancellationToken)
    {
        var cleanCode = request.Code.Trim().ToUpperInvariant();
        var coupon = await _dbContext.Coupons.FirstOrDefaultAsync(c => c.Code == cleanCode, cancellationToken);

        if (coupon == null)
        {
            return Result.Failure<ApplyCouponResultDto>(Error.NotFound("Coupon.NotFound", "Invalid promo coupon code."));
        }

        if (!coupon.IsValid(request.OrderAmount, out var errorMessage))
        {
            return Result.Failure<ApplyCouponResultDto>(Error.Validation("Coupon.Invalid", errorMessage));
        }

        var discountAmount = coupon.CalculateDiscount(request.OrderAmount);
        var finalAmount = Math.Max(0m, request.OrderAmount - discountAmount);

        coupon.IncrementUsage();
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = new ApplyCouponResultDto(
            coupon.Code,
            request.OrderAmount,
            discountAmount,
            finalAmount,
            coupon.IsPercentage,
            coupon.DiscountPercent);

        return Result.Success(result);
    }
}
