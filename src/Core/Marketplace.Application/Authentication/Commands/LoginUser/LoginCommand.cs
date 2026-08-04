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

    private readonly IIdentityDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(
        IIdentityDbContext dbContext,
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
            return Result.Failure<LoginResponse>(Error.Forbidden("Auth.AccountLocked", AuthMessages.AccountLocked));
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

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles, permissions);
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshToken = RefreshTokenEntity.Create(user.Id, refreshTokenValue, TimeSpan.FromDays(7), request.IpAddress);
        _dbContext.RefreshTokens.Add(refreshToken);

        var auditLog = AuditLog.Create("Login", "Success", request.IpAddress, request.UserAgent, user.Id);
        _dbContext.AuditLogs.Add(auditLog);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(accessToken, refreshTokenValue, false, user.Id));
    }
}
