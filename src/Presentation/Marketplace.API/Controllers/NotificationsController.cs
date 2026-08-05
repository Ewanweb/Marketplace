using System.Security.Claims;
using Marketplace.Application.Notifications.Commands.MarkNotificationAsRead;
using Marketplace.Application.Notifications.Queries.GetMyNotifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Authorize]
public class NotificationsController : ApiControllerBase
{
    /// <summary>
    /// Authenticated endpoint to fetch user's live notifications.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new GetMyNotificationsQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to mark a notification as read.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new MarkNotificationAsReadCommand(id, userId), cancellationToken);
        return HandleResult(result);
    }
}
