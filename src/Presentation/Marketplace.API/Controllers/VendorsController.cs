using System.Security.Claims;
using Marketplace.Application.Vendors.Commands.ApproveVendor;
using Marketplace.Application.Vendors.Commands.RegisterVendor;
using Marketplace.Application.Vendors.Queries;
using Marketplace.Application.Vendors.Queries.GetVendors;
using Marketplace.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class VendorsController : ApiControllerBase
{
    /// <summary>
    /// Public endpoint to list vendors / shops.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVendors([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await Sender.Send(new GetVendorsQuery(search, page, pageSize), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Retrieves vendors that the current user has access to.
    /// </summary>
    [HttpGet("mine")]
    [HasPermission("Products.Create")] // If they can create products, they need to see their vendors
    public async Task<IActionResult> GetMyVendors(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetMyVendorsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to register current user as a vendor.
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterVendor([FromBody] RegisterVendorRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new RegisterVendorCommand(
            userId,
            request.ShopNameEn,
            request.ShopNamePrs,
            request.ShopNamePs,
            request.Description,
            request.Description,
            request.Description,
            request.BankAccountInfo,
            request.LogoUrl ?? "",
            request.BannerUrl ?? "",
            request.KycDetailsJson ?? "");

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to approve a pending vendor registration.
    /// </summary>
    [HttpPut("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveVendor(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        var command = new ApproveVendorCommand(id, reason);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to reject a pending vendor registration.
    /// </summary>
    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectVendor(Guid id, [FromQuery] string? reason, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.RejectVendor.RejectVendorCommand(id, reason);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to submit a vendor update request.
    /// </summary>
    [HttpPut("{id:guid}/updates")]
    [Authorize]
    public async Task<IActionResult> SubmitVendorUpdate(Guid id, [FromBody] SubmitVendorUpdateRequest request, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.UpdateVendor.SubmitVendorUpdateCommand(
            id,
            request.ShopNameEn,
            request.ShopNamePrs,
            request.ShopNamePs,
            request.DescriptionEn,
            request.DescriptionPrs,
            request.DescriptionPs,
            request.LogoUrl ?? "",
            request.BannerUrl ?? "",
            request.BankAccountInfo ?? "");

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to approve a pending vendor update.
    /// </summary>
    [HttpPut("{id:guid}/updates/approve")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ApproveVendorUpdate(Guid id, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.UpdateVendor.ApproveVendorUpdateCommand(id);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Admin endpoint to reject a pending vendor update.
    /// </summary>
    [HttpPut("{id:guid}/updates/reject")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> RejectVendorUpdate(Guid id, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.UpdateVendor.RejectVendorUpdateCommand(id);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record RegisterVendorRequest(
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string Description,
    string BankAccountInfo,
    string? LogoUrl,
    string? BannerUrl,
    string? KycDetailsJson);

public record SubmitVendorUpdateRequest(
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    string? LogoUrl,
    string? BannerUrl,
    string? BankAccountInfo);
