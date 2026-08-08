using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Vendors.Queries.GetVendorMembers;

public sealed record VendorMemberDto(
    Guid Id,
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string Status,
    DateTime CreatedAt);

public sealed record GetVendorMembersQuery(Guid VendorId) : IRequest<Result<List<VendorMemberDto>>>;

public sealed class GetVendorMembersQueryHandler : IRequestHandler<GetVendorMembersQuery, Result<List<VendorMemberDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetVendorMembersQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<VendorMemberDto>>> Handle(GetVendorMembersQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId == null)
        {
            return Result.Failure<List<VendorMemberDto>>(Error.Unauthorized("Auth.Unauthorized", "User is not authenticated."));
        }

        var isOwnerOrMember = _currentUserService.IsSuperAdmin ||
                              await _dbContext.Vendors.AnyAsync(v => v.Id == request.VendorId && v.UserId == _currentUserService.UserId, cancellationToken) ||
                              await _dbContext.VendorMembers.AnyAsync(vm => vm.VendorId == request.VendorId && vm.UserId == _currentUserService.UserId, cancellationToken);

        if (!isOwnerOrMember)
        {
            return Result.Failure<List<VendorMemberDto>>(Error.Forbidden("Vendor.Forbidden", "You do not have permission to view members for this vendor."));
        }

        var rawMembers = await (from vm in _dbContext.VendorMembers.AsNoTracking()
                                 join u in _dbContext.Users.AsNoTracking() on vm.UserId equals u.Id
                                 where vm.VendorId == request.VendorId
                                 select new
                                 {
                                     vm.Id,
                                     UserId = u.Id,
                                     u.FullName,
                                     u.Email,
                                     vm.Role,
                                     vm.Status,
                                     vm.CreatedAt
                                 }).ToListAsync(cancellationToken);

        var members = rawMembers.Select(x => new VendorMemberDto(
            x.Id,
            x.UserId,
            x.FullName,
            x.Email,
            x.Role.ToString(),
            x.Status.ToString(),
            x.CreatedAt
        )).ToList();

        return Result.Success(members);
    }
}
