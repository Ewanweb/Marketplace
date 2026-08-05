using Marketplace.Application.Reports.Queries.GetFinancialReport;
using Marketplace.Application.Reports.Queries.GetOrderInvoice;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Authorize]
public class ReportsController : ApiControllerBase
{
    /// <summary>
    /// Authenticated endpoint to fetch executive platform financial analytics.
    /// </summary>
    [HttpGet("financial")]
    [HasPermission("Reports.Financial")]
    public async Task<IActionResult> GetFinancialReport(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetFinancialReportQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to fetch official invoice data for a specific order.
    /// </summary>
    [HttpGet("invoice/{orderId:guid}")]
    [HasPermission("Reports.Invoice")]
    public async Task<IActionResult> GetOrderInvoice(Guid orderId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetOrderInvoiceQuery(orderId), cancellationToken);
        return HandleResult(result);
    }
}
