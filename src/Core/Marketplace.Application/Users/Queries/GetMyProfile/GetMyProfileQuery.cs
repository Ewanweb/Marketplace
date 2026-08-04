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
            .FirstOrDefaultAsync(v => v.UserId == user.Id, cancellationToken);

        var profile = new UserProfileDto(
            user.Id,
            user.Email,
            user.FullName,
            user.PhoneNumber ?? "",
            user.IsEmailConfirmed,
            user.IsTwoFactorEnabled,
            roles,
            vendor?.Id);

        return Result.Success(profile);
    }
}
