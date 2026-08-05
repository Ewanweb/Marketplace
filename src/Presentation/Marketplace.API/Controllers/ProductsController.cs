using Marketplace.Application.Catalog.Commands.CreateProduct;
using Marketplace.Application.Catalog.Queries.GetProducts;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class ProductsController : ApiControllerBase
{
    /// <summary>
    /// Retrieves active product catalog with advanced search, filtering, and sorting parameters.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? vendorId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sortBy,
        CancellationToken cancellationToken)
    {
        var result = await Sender.Send(
            new GetProductsQuery(search, categoryId, vendorId, minPrice, maxPrice, sortBy),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to create a new product item.
    /// </summary>
    [HttpPost]
    [HasPermission("Products.Create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
