using Marketplace.Application.Catalog.Commands.CreateProduct;
using Marketplace.Application.Catalog.Queries.GetProducts;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves active product catalog with optional search query and category filter.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] string? search, [FromQuery] Guid? categoryId, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetProductsQuery(search, categoryId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to create a new product item.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
