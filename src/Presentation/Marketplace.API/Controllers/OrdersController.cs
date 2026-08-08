using System.Security.Claims;
using Marketplace.Application.Orders.Commands.CreateOrder;
using Marketplace.Application.Orders.Queries.GetMyOrders;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.API.Controllers;

public class OrdersController : ApiControllerBase
{
    /// <summary>
    /// Customer endpoint to place a new order.
    /// </summary>
    [HttpPost]
    [HasPermission("Orders.Create")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            command = command with { UserId = userId };
        }

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated customer endpoint to retrieve personal order history.
    /// </summary>
    [HttpGet("my")]
    [HasPermission("Orders.ViewOwn")]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await Sender.Send(new GetMyOrdersQuery(userId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Vendor endpoint to retrieve orders containing their items.
    /// </summary>
    [HttpGet("vendor")]
    [HasPermission("Products.Create")] // Same permission as vendor portal access
    public async Task<IActionResult> GetVendorOrders(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // We need the vendor ID associated with this user. For now we assume a query gets it or we fetch it.
        // Actually, let's look it up via MediatR if we can, or just let a generic vendor query handle it.
        // Wait, the handler for GetVendorOrdersQuery expects VendorId. I need to get the VendorId for the current user.
        return await ProcessVendorOrders(userId, cancellationToken);
    }

    private async Task<IActionResult> ProcessVendorOrders(Guid userId, CancellationToken cancellationToken)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Marketplace.Application.Common.Interfaces.IApplicationDbContext>();
        
        var vendorId = await dbContext.Vendors
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (vendorId == Guid.Empty)
        {
            var memberVendorId = await dbContext.VendorMembers
                .Where(vm => vm.UserId == userId)
                .Select(vm => vm.VendorId)
                .FirstOrDefaultAsync(cancellationToken);
                
            vendorId = memberVendorId;
        }

        if (vendorId == Guid.Empty)
        {
            var isUserAdmin = User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || 
                              User.Claims.Any(c => c.Type == ClaimTypes.Role && (c.Value == "SuperAdmin" || c.Value == "Admin"));
            if (isUserAdmin)
            {
                var allOrdersResult = await Sender.Send(new Marketplace.Application.Orders.Queries.GetVendorOrders.GetVendorOrdersQuery(Guid.Empty), cancellationToken);
                return HandleResult(allOrdersResult);
            }
            return Forbid();
        }

        var result = await Sender.Send(new Marketplace.Application.Orders.Queries.GetVendorOrders.GetVendorOrdersQuery(vendorId), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Vendor endpoint to update an order item's status.
    /// </summary>
    [HttpPut("items/{itemId:guid}/status")]
    [HasPermission("Products.Create")]
    public async Task<IActionResult> UpdateOrderItemStatus(Guid itemId, [FromBody] UpdateOrderItemStatusRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        using var scope = HttpContext.RequestServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<Marketplace.Application.Common.Interfaces.IApplicationDbContext>();
        
        var vendorId = await dbContext.Vendors
            .Where(v => v.UserId == userId)
            .Select(v => v.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (vendorId == Guid.Empty)
        {
            var memberVendorId = await dbContext.VendorMembers
                .Where(vm => vm.UserId == userId)
                .Select(vm => vm.VendorId)
                .FirstOrDefaultAsync(cancellationToken);
                
            vendorId = memberVendorId;
        }

        if (vendorId == Guid.Empty)
        {
            return Forbid();
        }

        var command = new Marketplace.Application.Orders.Commands.UpdateOrderItemStatus.UpdateOrderItemStatusCommand(
            itemId, vendorId, request.Status);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateOrderItemStatusRequest(Marketplace.Domain.Entities.OrderStatus Status);
