using Marketplace.Application.Admin.Queries.GetAdminDashboardStats;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Route("api/v1/admin/dashboard")]
[Authorize]
public class AdminDashboardController : ApiControllerBase
{
    /// <summary>
    /// Retrieves admin dashboard analytics & metrics overview.
    /// </summary>
    [HttpGet("stats")]
    [HasPermission("Reports.Financial")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAdminDashboardStatsQuery(), cancellationToken);
        return HandleResult(result);
    }
}
