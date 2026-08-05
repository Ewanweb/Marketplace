using Marketplace.Application.Coupons.Commands.ApplyCoupon;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class CouponsController : ApiControllerBase
{
    /// <summary>
    /// Validates a promo coupon code and calculates instant order discounts.
    /// </summary>
    [HttpPost("apply")]
    public async Task<IActionResult> ApplyCoupon([FromBody] ApplyCouponRequest request, CancellationToken cancellationToken)
    {
        var command = new ApplyCouponCommand(request.Code, request.OrderAmount);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record ApplyCouponRequest(string Code, decimal OrderAmount);
