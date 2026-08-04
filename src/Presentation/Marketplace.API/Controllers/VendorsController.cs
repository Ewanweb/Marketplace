using System.Security.Claims;
using Marketplace.Application.Vendors.Commands.RegisterVendor;
using Marketplace.Application.Vendors.Queries.GetVendors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class VendorsController : ApiControllerBase
{
    /// <summary>
    /// Public endpoint to list vendors / shops.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVendors([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetVendorsQuery(search), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to register current user as a vendor.
    /// </summary>
    [HttpPost("register")]
    [Authorize]
    public async Task<IActionResult> RegisterVendor([FromBody] RegisterVendorRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
            request.BankAccountInfo,
            request.LogoUrl ?? "",
            request.BannerUrl ?? "");

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
    string? BannerUrl);
