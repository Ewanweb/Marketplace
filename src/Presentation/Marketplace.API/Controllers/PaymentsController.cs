using System.Security.Claims;
using Marketplace.Application.Payments.Commands.ProcessPayment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Authorize]
public class PaymentsController : ApiControllerBase
{
    /// <summary>
    /// Authenticated customer endpoint to process order payment and split vendor commission.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new ProcessPaymentCommand(
            request.OrderId,
            userId,
            request.PaymentMethod ?? "CreditCard");

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record ProcessPaymentRequest(Guid OrderId, string? PaymentMethod);
