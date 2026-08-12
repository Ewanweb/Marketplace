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
    [Authorize]
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

    /// <summary>
    /// Gets members of a specific vendor shop.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [Authorize]
    public async Task<IActionResult> GetVendorMembers(Guid id, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new Marketplace.Application.Vendors.Queries.GetVendorMembers.GetVendorMembersQuery(id), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Adds a member to a vendor shop by user email.
    /// </summary>
    [HttpPost("{id:guid}/members")]
    [Authorize]
    public async Task<IActionResult> AddVendorMember(Guid id, [FromBody] AddVendorMemberRequest request, CancellationToken cancellationToken)
    {
        var role = Enum.TryParse<Marketplace.Domain.Entities.VendorRole>(request.Role, true, out var parsedRole) ? parsedRole : Marketplace.Domain.Entities.VendorRole.Staff;
        var command = new Marketplace.Application.Vendors.Commands.AddVendorMember.AddVendorMemberCommand(id, request.UserEmail, role);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Removes a member from a vendor shop.
    /// </summary>
    [HttpDelete("{id:guid}/members/{memberId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveVendorMember(Guid id, Guid memberId, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.RemoveVendorMember.RemoveVendorMemberCommand(id, memberId);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Gets pending vendor invitations for the logged-in user.
    /// </summary>
    [HttpGet("invitations")]
    [Authorize]
    public async Task<IActionResult> GetMyVendorInvitations(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new Marketplace.Application.Vendors.Queries.GetMyVendorInvitations.GetMyVendorInvitationsQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Responds (accept/reject) to a vendor shop invitation.
    /// </summary>
    [HttpPut("invitations/{memberId:guid}/respond")]
    [Authorize]
    public async Task<IActionResult> RespondVendorInvitation(Guid memberId, [FromBody] RespondInvitationRequest request, CancellationToken cancellationToken)
    {
        var command = new Marketplace.Application.Vendors.Commands.RespondVendorInvitation.RespondVendorInvitationCommand(memberId, request.Accept);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticated endpoint to update vendor's affiliate commission rate.
    /// </summary>
    [HttpPut("{id:guid}/affiliate-rate")]
    [Authorize]
    public async Task<IActionResult> UpdateAffiliateCommissionRate(Guid id, [FromBody] UpdateAffiliateCommissionRateRequest request, CancellationToken cancellationToken)
    {
        // TODO: Verify if user has permission to update this vendor (Owner/Admin)
        var command = new Marketplace.Application.Affiliates.Commands.UpdateAffiliateCommissionRateCommand(id, request.NewRate);
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateAffiliateCommissionRateRequest(decimal NewRate);
public record RespondInvitationRequest(bool Accept);
public record AddVendorMemberRequest(string UserEmail, string? Role);

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
