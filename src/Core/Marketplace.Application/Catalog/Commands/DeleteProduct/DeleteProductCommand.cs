using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRedisCacheService _cacheService;

    public DeleteProductCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IRedisCacheService cacheService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));
        }

        if (!_currentUserService.IsSuperAdmin)
        {
            var isMember = await _dbContext.VendorMembers
                .AnyAsync(vm => vm.VendorId == product.VendorId && vm.UserId == _currentUserService.UserId, cancellationToken);
            
            if (!isMember)
            {
                return Result.Failure(Error.Forbidden("Product.Forbidden", "You do not have permission to delete this product."));
            }
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cacheService.InvalidateProductsCacheAsync(cancellationToken);

        return Result.Success();
    }
}
