using System.Security.Claims;
using Marketplace.Application.Users.Queries.GetMyProfile;
using Marketplace.Application.Users.Commands.UpdateUserProfile;
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
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new GetMyProfileQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    public sealed record UpdateProfileRequest(string FullName, string? PhoneNumber, string? Address);

    /// <summary>
    /// Authenticated endpoint to update the current logged-in user profile.
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new UpdateUserProfileCommand(userId, request.FullName, request.PhoneNumber, request.Address);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
