using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Queries.GetMyVendorInvitations;

public sealed record VendorInvitationDto(
    Guid MemberId,
    Guid VendorId,
    string ShopNameEn,
    string ShopNamePrs,
    string ShopNamePs,
    string Role,
    DateTime CreatedAt);

public sealed record GetMyVendorInvitationsQuery() : IRequest<Result<List<VendorInvitationDto>>>;

public sealed class GetMyVendorInvitationsQueryHandler : IRequestHandler<GetMyVendorInvitationsQuery, Result<List<VendorInvitationDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyVendorInvitationsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<VendorInvitationDto>>> Handle(GetMyVendorInvitationsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure<List<VendorInvitationDto>>(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var rawInvitations = await (from vm in _dbContext.VendorMembers.AsNoTracking()
                                     join v in _dbContext.Vendors.AsNoTracking() on vm.VendorId equals v.Id
                                     where vm.UserId == _currentUserService.UserId && vm.Status == VendorMemberStatus.Pending
                                     select new
                                     {
                                         vm.Id,
                                         VendorId = v.Id,
                                         v.ShopNameEn,
                                         v.ShopNamePrs,
                                         v.ShopNamePs,
                                         vm.Role,
                                         vm.CreatedAt
                                     }).ToListAsync(cancellationToken);

        var invitations = rawInvitations.Select(x => new VendorInvitationDto(
            x.Id,
            x.VendorId,
            x.ShopNameEn,
            x.ShopNamePrs,
            x.ShopNamePs,
            x.Role.ToString(),
            x.CreatedAt
        )).ToList();

        return Result.Success(invitations);
    }
}
