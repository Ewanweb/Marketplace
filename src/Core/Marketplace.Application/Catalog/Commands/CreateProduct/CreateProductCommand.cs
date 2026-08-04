using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;

namespace Marketplace.Application.Catalog.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string TitleEn,
    string TitlePrs,
    string TitlePs,
    string DescriptionEn,
    string DescriptionPrs,
    string DescriptionPs,
    decimal Price,
    int StockQuantity,
    string ImageUrl,
    Guid CategoryId) : IRequest<Result<Guid>>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IIdentityDbContext _dbContext;

    public CreateProductCommandHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.TitleEn,
            string.IsNullOrWhiteSpace(request.TitlePrs) ? request.TitleEn : request.TitlePrs,
            string.IsNullOrWhiteSpace(request.TitlePs) ? request.TitleEn : request.TitlePs,
            request.DescriptionEn,
            string.IsNullOrWhiteSpace(request.DescriptionPrs) ? request.DescriptionEn : request.DescriptionPrs,
            string.IsNullOrWhiteSpace(request.DescriptionPs) ? request.DescriptionEn : request.DescriptionPs,
            request.Price,
            request.StockQuantity,
            request.ImageUrl,
            request.CategoryId
        );

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
