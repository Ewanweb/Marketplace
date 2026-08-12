using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string NameEn,
    string NamePrs,
    string NamePs,
    string IconName,
    Guid? ParentId) : IRequest<Result<Guid>>;

public sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateCategoryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.NameEn))
            return Result.Failure<Guid>(Error.Validation("NameEn.Empty", "English name is required."));

        int level = 1;
        if (request.ParentId.HasValue)
        {
            var parent = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Id == request.ParentId.Value, cancellationToken);
            if (parent == null)
            {
                return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Parent category not found."));
            }
            if (parent.Level >= 3)
            {
                return Result.Failure<Guid>(Error.Validation("Category.LevelMax", "Cannot create category beyond level 3."));
            }
            level = parent.Level + 1;
        }

        var category = Category.Create(
            request.NameEn,
            request.NamePrs,
            request.NamePs,
            request.IconName ?? string.Empty,
            request.ParentId,
            level);

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}
