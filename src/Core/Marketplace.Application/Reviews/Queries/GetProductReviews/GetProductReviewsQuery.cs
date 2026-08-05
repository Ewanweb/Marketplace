using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Reviews.Queries.GetProductReviews;

public sealed record ReviewDto(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ProductId,
    int Rating,
    string Comment,
    bool IsVerifiedPurchase,
    DateTime CreatedAt);

public sealed record GetProductReviewsQuery(Guid ProductId) : IRequest<Result<List<ReviewDto>>>;

public sealed class GetProductReviewsQueryHandler : IRequestHandler<GetProductReviewsQuery, Result<List<ReviewDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductReviewsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<ReviewDto>>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await (from r in _dbContext.Reviews.AsNoTracking()
                             join u in _dbContext.Users.AsNoTracking() on r.UserId equals u.Id
                             where r.ProductId == request.ProductId
                             orderby r.CreatedAt descending
                             select new ReviewDto(
                                 r.Id,
                                 r.UserId,
                                 !string.IsNullOrEmpty(u.FullName) ? u.FullName : u.Email,
                                 r.ProductId,
                                 r.Rating,
                                 r.Comment,
                                 r.IsVerifiedPurchase,
                                 r.CreatedAt)).ToListAsync(cancellationToken);

        return Result.Success(reviews);
    }
}
