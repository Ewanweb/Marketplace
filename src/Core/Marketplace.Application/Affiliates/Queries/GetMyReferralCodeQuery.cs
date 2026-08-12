using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public sealed record GetMyReferralCodeQuery() : IRequest<Result<string>>;

public sealed class GetMyReferralCodeQueryHandler : IRequestHandler<GetMyReferralCodeQuery, Result<string>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReferralCodeQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(GetMyReferralCodeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
            
        if (userId == null)
        {
            return Result.Failure<string>(Error.Unauthorized("Unauthorized", "User not logged in."));
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<string>(Error.NotFound("User.NotFound", "User not found."));
        }

        return Result.Success(user.ReferralCode);
    }
}
