using System.Globalization;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Affiliates.Queries;

public sealed record MarketerProductDto(
    Guid ProductId,
    string Title,
    string ImageUrl,
    decimal Price,
    string VendorName,
    string ReferralCode);

public sealed record GetMarketerStoreProductsQuery() : IRequest<Result<List<MarketerProductDto>>>;

public sealed class GetMarketerStoreProductsQueryHandler : IRequestHandler<GetMarketerStoreProductsQuery, Result<List<MarketerProductDto>>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMarketerStoreProductsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<MarketerProductDto>>> Handle(GetMarketerStoreProductsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Result.Failure<List<MarketerProductDto>>(Error.Unauthorized("Unauthorized", "User not logged in."));
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || string.IsNullOrWhiteSpace(user.ReferralCode))
        {
            return Result.Failure<List<MarketerProductDto>>(Error.NotFound("User.NotFound", "User not found or no referral code."));
        }

        var marketerVendorIds = await _dbContext.VendorMembers
            .Where(vm => vm.UserId == userId && vm.Role == Domain.Entities.VendorRole.Marketer && vm.Status == Domain.Entities.VendorMemberStatus.Accepted)
            .Select(vm => vm.VendorId)
            .ToListAsync(cancellationToken);

        if (marketerVendorIds.Count == 0)
        {
            return Result.Success(new List<MarketerProductDto>());
        }

        var culture = CultureInfo.CurrentUICulture.Name;
        
        var products = await _dbContext.Products
            .Include(p => p.Vendor)
            .Where(p => p.IsActive && marketerVendorIds.Contains(p.VendorId))
            .ToListAsync(cancellationToken);

        var dtos = products.Select(p => new MarketerProductDto(
            p.Id,
            p.GetTitle(culture),
            p.ImageUrl,
            p.Price,
            p.Vendor?.GetShopName(culture) ?? "Vendor",
            user.ReferralCode
        )).ToList();

        return Result.Success(dtos);
    }
}
