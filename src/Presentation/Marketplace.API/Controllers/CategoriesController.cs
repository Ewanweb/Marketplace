using Marketplace.Application.Catalog.Commands.CreateCategory;
using Marketplace.Application.Catalog.Commands.UpdateCategory;
using Marketplace.Application.Catalog.Commands.DeleteCategory;
using Marketplace.Application.Catalog.Queries.GetCategories;
using Microsoft.AspNetCore.Authorization;
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

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,SystemAdmin")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new DeleteCategoryCommand(id), cancellationToken);
        return HandleResult(result);
    }
}
