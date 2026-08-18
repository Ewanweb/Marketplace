using FluentValidation;
using Marketplace.Application.Authentication.Common;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Marketplace.Domain.Entities.RefreshToken;

namespace Marketplace.Application.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenResponse(string AccessToken, string RefreshToken);

public sealed record RefreshTokenCommand(string Token, string IpAddress) : IRequest<Result<RefreshTokenResponse>>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenCommandHandler(IApplicationDbContext dbContext, IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var existingToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
                .ThenInclude(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(Error.Unauthorized("Auth.InvalidToken", AuthMessages.InvalidToken));
        }

        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.Revoke(request.IpAddress, newRefreshTokenValue);

        var newRefreshToken = RefreshTokenEntity.Create(existingToken.UserId, newRefreshTokenValue, TimeSpan.FromDays(7), request.IpAddress);
        _dbContext.RefreshTokens.Add(newRefreshToken);

        var user = existingToken.User;
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        // Include vendor IDs where user is verified owner or staff member
        var ownedVendorIds = await _dbContext.Vendors
            .Where(v => v.UserId == user.Id && v.IsVerified)
            .Select(v => v.Id)
            .ToListAsync(cancellationToken);

        var memberRecords = await _dbContext.VendorMembers
            .Where(vm => vm.UserId == user.Id && vm.Status == Domain.Entities.VendorMemberStatus.Accepted)
            .ToListAsync(cancellationToken);

        var staffOrOwnerVendorIds = memberRecords
            .Where(vm => vm.Role == Domain.Entities.VendorRole.Owner || vm.Role == Domain.Entities.VendorRole.Staff)
            .Select(vm => vm.VendorId)
            .ToList();

        if (ownedVendorIds.Count > 0 || memberRecords.Any(vm => vm.Role == Domain.Entities.VendorRole.Owner))
        {
            if (!roles.Contains("Vendor")) roles.Add("Vendor");
        }

        if (memberRecords.Any(vm => vm.Role == Domain.Entities.VendorRole.Staff))
        {
            if (!roles.Contains("Staff")) roles.Add("Staff");
        }

        if (memberRecords.Any(vm => vm.Role == Domain.Entities.VendorRole.Marketer))
        {
            if (!roles.Contains("Marketer")) roles.Add("Marketer");
        }

        var allVendorIds = ownedVendorIds.Union(staffOrOwnerVendorIds).Distinct().ToList();

        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, allVendorIds);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse(newAccessToken, newRefreshTokenValue));
    }
}
