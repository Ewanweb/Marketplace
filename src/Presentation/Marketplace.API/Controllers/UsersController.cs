using System.Security.Claims;
using Marketplace.Application.Users.Queries.GetMyProfile;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Authorize]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Authenticated endpoint to fetch the current logged-in user profile.
    /// </summary>
    [HttpGet("me")]
    [HasPermission("Users.ViewProfile")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new GetMyProfileQuery(userId), cancellationToken);
        return HandleResult(result);
    }
}
