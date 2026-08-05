using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid UserId,
    Guid ProductId,
    int Rating,
    string Comment) : IRequest<Result<Guid>>;

public sealed class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(1000);
    }
}

public sealed class CreateReviewCommandHandler : IRequestHandler<CreateReviewCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateReviewCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Failure<Guid>(Error.NotFound("Product.NotFound", "Product not found."));
        }

        var hasPurchased = await _dbContext.OrderItems
            .AnyAsync(oi => oi.ProductId == request.ProductId
                && _dbContext.Orders.Any(o => o.Id == oi.OrderId && o.UserId == request.UserId),
                cancellationToken);

        if (!hasPurchased)
        {
            return Result.Failure<Guid>(Error.Forbidden("Review.NotPurchased",
                "You can only review products you have purchased."));
        }

        var review = Review.Create(
            request.UserId,
            request.ProductId,
            product.VendorId,
            request.Rating,
            request.Comment,
            isVerifiedPurchase: hasPurchased);

        _dbContext.Reviews.Add(review);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Recalculate Product Rating Average
        var allProductReviews = await _dbContext.Reviews
            .Where(r => r.ProductId == request.ProductId)
            .Select(r => r.Rating)
            .ToListAsync(cancellationToken);

        if (allProductReviews.Count > 0)
        {
            var avgRating = Math.Round(allProductReviews.Average(), 1);
            product.UpdateRating(avgRating);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(review.Id);
    }
}
