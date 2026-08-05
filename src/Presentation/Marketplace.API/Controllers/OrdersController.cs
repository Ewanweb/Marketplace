using System.Security.Claims;
using Marketplace.Application.Orders.Commands.CreateOrder;
using Marketplace.Application.Orders.Queries.GetMyOrders;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class OrdersController : ApiControllerBase
{
    /// <summary>
    /// Customer endpoint to place a new order.
    /// </summary>
    [HttpPost]
    [HasPermission("Orders.Create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            command = command with { UserId = userId };
        }

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated customer endpoint to retrieve personal order history.
    /// </summary>
    [HttpGet("my")]
    [HasPermission("Orders.ViewOwn")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new GetMyOrdersQuery(userId), cancellationToken);
        return HandleResult(result);
    }
}
