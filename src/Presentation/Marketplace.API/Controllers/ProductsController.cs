using System.Security.Claims;
using Marketplace.Application.Catalog.Commands.CreateProduct;
using Marketplace.Application.Catalog.Commands.UpdateProduct;
using Marketplace.Application.Catalog.Queries.GetProducts;
using Marketplace.API.Authorization;
using Marketplace.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetProductsQuery(search, categoryId, vendorId, minPrice, maxPrice, sortBy, page, pageSize),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves product catalog for admin/vendor dashboard, filtered by permissions.
    /// </summary>
    [HttpGet("admin")]
    [HasPermission("Products.Create")]
    public async Task<IActionResult> GetAdminProducts(
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] Guid? vendorId,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(
            new GetAdminProductsQuery(search, categoryId, vendorId, sortBy, page, pageSize),
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
        if (command.VendorId == Guid.Empty)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                using var scope = HttpContext.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<Marketplace.Application.Common.Interfaces.IApplicationDbContext>();

                var vendorId = await dbContext.Vendors
                    .Where(v => v.UserId == userId && v.IsVerified)
                    .Select(v => v.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (vendorId == Guid.Empty)
                {
                    vendorId = await dbContext.VendorMembers
                        .Where(vm => vm.UserId == userId)
                        .Select(vm => vm.VendorId)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                if (vendorId != Guid.Empty)
                {
                    command = command with { VendorId = vendorId };
                }
            }
        }

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to update an existing product item.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("Products.Create")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(Error.Validation("Product.IdMismatch", "Product ID mismatch."));
        }

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to delete an existing product item.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("Products.Create")] // Same permission level used for create/update
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Catalog.Commands.DeleteProduct.DeleteProductCommand(id);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
