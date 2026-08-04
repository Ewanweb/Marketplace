using Marketplace.Application.Catalog.Queries.GetCategories;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class CategoriesController : ApiControllerBase
{
    /// <summary>
    /// Retrieves all active product categories with dynamic trilingual names.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetCategoriesQuery(), cancellationToken);
        return HandleResult(result);
    }
}
