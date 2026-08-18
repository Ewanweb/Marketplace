using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string NameEn,
    string NamePrs,
    string NamePs,
    string IconName,
    string? ImageUrl,
    Guid? ParentId) : IRequest<Result>;

public sealed class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IRedisCacheService _cacheService;

    public UpdateCategoryCommandHandler(IApplicationDbContext dbContext, IRedisCacheService cacheService)
    {
        _dbContext = dbContext;
        _cacheService = cacheService;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null)
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));

        if (request.ParentId == request.Id)
            return Result.Failure(Error.Validation("Category.InvalidParent", "A category cannot be its own parent."));

        int level = 1;
        if (request.ParentId.HasValue)
        {
            var parent = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.ParentId.Value, cancellationToken);
            if (parent == null)
                return Result.Failure(Error.NotFound("Category.NotFound", "Parent category not found."));
            
            if (parent.Level >= 3)
                return Result.Failure(Error.Validation("Category.LevelMax", "Cannot nest beyond level 3."));
                
            level = parent.Level + 1;
        }

        category.Update(
            request.NameEn,
            request.NamePrs,
            request.NamePs,
            request.IconName ?? string.Empty,
            request.ImageUrl,
            request.ParentId,
            level);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _cacheService.InvalidateCategoriesCacheAsync(cancellationToken);

        return Result.Success();
    }
}
