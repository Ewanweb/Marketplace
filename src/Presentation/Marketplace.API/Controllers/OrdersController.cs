using Marketplace.Application.Orders.Commands.CreateOrder;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class OrdersController : ApiControllerBase
{
    /// <summary>
    /// Customer endpoint to place a new order.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
