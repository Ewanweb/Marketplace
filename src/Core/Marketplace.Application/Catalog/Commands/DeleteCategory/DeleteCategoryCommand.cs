using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRedisCacheService _cacheService;

    public DeleteCategoryCommandHandler(IApplicationDbContext dbContext, IRedisCacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            
        if (category == null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        if (category.SubCategories.Any(sc => sc.IsActive))
            return Result.Failure(Error.Validation("Category.HasSubcategories", "Cannot delete category because it has active subcategories."));

        if (category.Products.Any(p => p.IsActive))
            return Result.Failure(Error.Validation("Category.HasProducts", "Cannot delete category because it has active products."));

        category.Deactivate();

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cacheService.InvalidateCategoriesCacheAsync(cancellationToken);

        return Result.Success();
    }
}
