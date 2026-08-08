using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Commands.RemoveVendorMember;

public sealed record RemoveVendorMemberCommand(Guid VendorId, Guid MemberId) : IRequest<Result>;

public sealed class RemoveVendorMemberCommandValidator : AbstractValidator<RemoveVendorMemberCommand>
{
    public RemoveVendorMemberCommandValidator()
    {
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public sealed class RemoveVendorMemberCommandHandler : IRequestHandler<RemoveVendorMemberCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public RemoveVendorMemberCommandHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RemoveVendorMemberCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var isOwner = _currentUserService.IsSuperAdmin ||
                      await _dbContext.Vendors.AnyAsync(v => v.Id == request.VendorId && v.UserId == _currentUserService.UserId, cancellationToken) ||
                      await _dbContext.VendorMembers.AnyAsync(vm => vm.VendorId == request.VendorId && vm.UserId == _currentUserService.UserId && vm.Role == VendorRole.Owner, cancellationToken);

        if (!isOwner)
        {
            return Result.Failure(Error.Forbidden("Vendor.Forbidden", "Only the vendor owner or admin can remove members."));
        }

        var member = await _dbContext.VendorMembers.FirstOrDefaultAsync(vm => vm.Id == request.MemberId && vm.VendorId == request.VendorId, cancellationToken);
        if (member == null)
        {
            return Result.Failure(Error.NotFound("VendorMember.NotFound", "Member not found in this vendor shop."));
        }

        if (member.Role == VendorRole.Owner)
        {
            return Result.Failure(Error.Conflict("VendorMember.CannotRemoveOwner", Marketplace.Shared.Localization.LocalizedMessage.Get(
                "Cannot remove the shop owner.",
                "امکان حذف مالکین اصلی فروشگاه وجود ندارد.",
                "د پلورنځي اصلي مالک حذف کول امکان نلري."
            )));
        }

        _dbContext.VendorMembers.Remove(member);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
