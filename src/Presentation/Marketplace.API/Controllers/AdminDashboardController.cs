using Marketplace.Application.Admin.Queries.GetAdminDashboardStats;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Route("api/v1/admin/dashboard")]
public class AdminDashboardController : ApiControllerBase
{
    /// <summary>
    /// Retrieves admin dashboard analytics & metrics overview.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetAdminDashboardStatsQuery(), cancellationToken);
        return HandleResult(result);
    }
}
