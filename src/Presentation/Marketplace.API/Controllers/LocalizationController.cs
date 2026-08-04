using Marketplace.Application.Localization.Queries.GetLocalizationStrings;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

[Route("api/v1/localization")]
public class LocalizationController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all localized UI strings dynamically based on the Accept-Language header (en, prs, ps).
    /// </summary>
    [HttpGet("strings")]
    public async Task<IActionResult> GetLocalizationStrings(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetLocalizationStringsQuery(), cancellationToken);
        return HandleResult(result);
    }
}
