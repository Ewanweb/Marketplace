using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public sealed record GetMyMarketerVendorsQuery() : IRequest<Result<List<Guid>>>;

public sealed class GetMyMarketerVendorsQueryHandler : IRequestHandler<GetMyMarketerVendorsQuery, Result<List<Guid>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMyMarketerVendorsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<Guid>>> Handle(GetMyMarketerVendorsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Failure<List<Guid>>(Error.Unauthorized("Unauthorized", "User not logged in."));
        }

        var marketerVendorIds = await _dbContext.VendorMembers
            .Where(vm => vm.UserId == userId && vm.Role == VendorRole.Marketer && vm.Status == VendorMemberStatus.Accepted)
            .Select(vm => vm.VendorId)
            .ToListAsync(cancellationToken);

        return Result.Success(marketerVendorIds);
    }
}
