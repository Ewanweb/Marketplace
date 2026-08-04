using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Authentication.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken, string IpAddress) : IRequest<Result>;

public sealed record RevokeAllTokensCommand(Guid UserId, string IpAddress) : IRequest<Result>;

public sealed class LogoutCommandHandler : 
    IRequestHandler<LogoutCommand, Result>,
    IRequestHandler<RevokeAllTokensCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public LogoutCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (token is null || token.IsRevoked)
        {
            return Result.Success();
        }

        token.Revoke(request.IpAddress);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Handle(RevokeAllTokensCommand request, CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == request.UserId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(request.IpAddress);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
