using FluentValidation;
using Marketplace.Application.Authentication.Common;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Authentication.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FullName,
    string Email,
    string? PhoneNumber,
    string Password) : IRequest<Result<Guid>>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(_ => AuthMessages.FullNameRequired)
            .MaximumLength(150);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(_ => AuthMessages.EmailRequired)
            .EmailAddress().WithMessage(_ => AuthMessages.InvalidEmailFormat);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_ => AuthMessages.PasswordRequired)
            .MinimumLength(8).WithMessage(_ => AuthMessages.PasswordMinLength)
            .Matches(@"[A-Z]").WithMessage(_ => AuthMessages.PasswordUppercase)
            .Matches(@"[a-z]").WithMessage(_ => AuthMessages.PasswordLowercase)
            .Matches(@"[0-9]").WithMessage(_ => AuthMessages.PasswordDigit)
            .Matches(@"[\^$*.\[\]{}()?\-""!@#%&/\\,><':;|_~`]").WithMessage(_ => AuthMessages.PasswordSpecialChar);
    }
}

public sealed class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    public RegisterUserCommandHandler(
        IApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.ToLowerInvariant().Trim();

        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            return Result.Failure<Guid>(Error.Conflict("User.EmailExists", AuthMessages.EmailExists));
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(normalizedEmail, passwordHash, request.FullName, request.PhoneNumber);

        var verificationToken = Guid.NewGuid().ToString("N");
        user.SetEmailVerificationToken(verificationToken, TimeSpan.FromHours(24));

        _dbContext.Users.Add(user);

        var customerRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Customer", cancellationToken);
        if (customerRole != null)
        {
            var userRole = UserRole.Create(user.Id, customerRole.Id);
            _dbContext.UserRoles.Add(userRole);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendEmailVerificationAsync(user.Email, verificationToken, cancellationToken);

        return Result.Success(user.Id);
    }
}
