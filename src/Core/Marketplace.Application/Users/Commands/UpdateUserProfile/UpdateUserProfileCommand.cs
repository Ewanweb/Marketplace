using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Users.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string FullName,
    string? PhoneNumber,
    string? Address) : IRequest<Result>;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
        RuleFor(x => x.Address).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Address));
    }
}

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateUserProfileCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));
        }

        user.UpdateProfile(request.FullName, request.PhoneNumber, request.Address);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
