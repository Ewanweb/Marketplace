using Marketplace.Application.Affiliates.Commands;
using Marketplace.Application.Affiliates.Queries;
using Marketplace.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class AffiliatesController : ApiControllerBase
{
    /// <summary>
    /// Gets the current logged-in user's referral code.
    /// </summary>
    [HttpGet("my-code")]
    [Authorize]
    public async Task<IActionResult> GetMyReferralCode(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMyReferralCodeQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets the current user's affiliate statistics.
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetMyAffiliateStats(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMyAffiliateStatsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets the list of referrals made by the current user.
    /// </summary>
    [HttpGet("referrals")]
    [Authorize]
    public async Task<IActionResult> GetMyReferrals(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMyReferralsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to change the status of an affiliate referral (e.g., mark as Paid).
    /// </summary>
    [HttpPut("referrals/{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateReferralStatus(Guid id, [FromBody] UpdateReferralStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AffiliateStatus>(request.Status, true, out var newStatus))
        {
            return BadRequest("Invalid status.");
        }

        var command = new UpdateReferralStatusCommand(id, newStatus);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateReferralStatusRequest(string Status);
