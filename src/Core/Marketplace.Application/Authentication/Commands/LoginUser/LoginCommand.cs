using FluentValidation;
using Marketplace.Application.Authentication.Common;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Marketplace.Domain.Entities.RefreshToken;

namespace Marketplace.Application.Authentication.Commands.LoginUser;

public sealed record LoginResponse(
    string? AccessToken,
    string? RefreshToken,
    bool RequiresTwoFactor,
    Guid? UserId);

public sealed record LoginCommand(
    string Email,
    string Password,
    string IpAddress,
    string UserAgent) : IRequest<Result<LoginResponse>>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => AuthMessages.EmailRequired)
            .EmailAddress().WithMessage(_ => AuthMessages.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => AuthMessages.PasswordRequired);
    }
}

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(Error.Unauthorized("Auth.InvalidCredentials", AuthMessages.InvalidCredentials));
        }

        if (user.IsLockedOut())
        {
            return Result.Failure<LoginResponse>(Error.Forbidden("Auth.AccountLocked", 
                "Your account is locked due to multiple failed login attempts. Please try again later."));
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            user.RecordFailedLoginAttempt(MaxFailedAttempts, LockoutDuration);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Failure<LoginResponse>(Error.Unauthorized("Auth.InvalidCredentials", AuthMessages.InvalidCredentials));
        }

        user.ResetFailedLoginCount();

        if (user.IsTwoFactorEnabled)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success(new LoginResponse(null, null, true, user.Id));
        }

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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions, allVendorIds);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, TimeSpan.FromDays(7), request.IpAddress);
        _dbContext.RefreshTokens.Add(refreshToken);

        var auditLog = AuditLog.Create("Login", "Success", request.IpAddress, request.UserAgent, user.Id);
        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(accessToken, refreshTokenValue, false, user.Id));
    }
}
