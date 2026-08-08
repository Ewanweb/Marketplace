using System.Security.Claims;
using Marketplace.Application.Reviews.Commands.CreateReview;
using Marketplace.Application.Reviews.Queries.GetProductReviews;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class ReviewsController : ApiControllerBase
{
    /// <summary>
    /// Public endpoint to get user reviews for a specific product item.
    /// </summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(Guid productId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductReviewsQuery(productId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated customer endpoint to submit a review and rating for a product.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new CreateReviewCommand(
            userId,
            request.ProductId,
            request.Rating,
            request.Comment);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record CreateReviewRequest(Guid ProductId, int Rating, string Comment);
