using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Users.Queries.GetMyProfile;

public sealed record UserProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string PhoneNumber,
    string? Address,
    bool IsEmailConfirmed,
    bool TwoFactorEnabled,
    List<string> Roles,
    Guid? VendorId = null);

public sealed record GetMyProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public sealed class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<UserProfileDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyProfileQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserProfileDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<UserProfileDto>(Error.NotFound("User.NotFound", "User not found."));
        }

        var roles = await (from ur in _dbContext.UserRoles
                           join r in _dbContext.Roles on ur.RoleId equals r.Id
                           where ur.UserId == user.Id
                           select r.Name).ToListAsync(cancellationToken);

        var vendor = await _dbContext.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == user.Id && v.IsVerified, cancellationToken);

        var memberRecord = await _dbContext.VendorMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(vm => vm.UserId == user.Id && vm.Status == Domain.Entities.VendorMemberStatus.Accepted, cancellationToken);

        Guid? activeManagedVendorId = null;

        if (vendor != null)
        {
            activeManagedVendorId = vendor.Id;
            if (!roles.Contains("Vendor")) roles.Add("Vendor");
        }
        else if (memberRecord != null)
        {
            if (memberRecord.Role == Domain.Entities.VendorRole.Owner)
            {
                activeManagedVendorId = memberRecord.VendorId;
                if (!roles.Contains("Vendor")) roles.Add("Vendor");
            }
            else if (memberRecord.Role == Domain.Entities.VendorRole.Staff)
            {
                activeManagedVendorId = memberRecord.VendorId;
                if (!roles.Contains("Staff")) roles.Add("Staff");
            }
            else if (memberRecord.Role == Domain.Entities.VendorRole.Marketer)
            {
                if (!roles.Contains("Marketer")) roles.Add("Marketer");
            }
        }

        var profile = new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber ?? "",
            user.Address,
            user.IsEmailConfirmed,
            user.IsTwoFactorEnabled,
            roles,
            activeManagedVendorId);

        return Result.Success(profile);
    }
}
