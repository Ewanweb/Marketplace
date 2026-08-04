using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
    Guid CategoryId,
    Guid VendorId,
    string AvailableSizes,
    string AvailableColors) : IRequest<Result<Guid>>;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TitlePrs).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TitlePs).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
    }
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateProductCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Category not found."));
        }

        var vendorExists = await _dbContext.Vendors
            .AnyAsync(v => v.Id == request.VendorId, cancellationToken);
            
        if (!vendorExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        var product = Product.Create(
            request.TitleEn,
            request.TitlePrs,
            request.TitlePs,
            request.DescriptionEn,
            request.DescriptionPrs,
            request.DescriptionPs,
            request.Price,
            request.StockQuantity,
            request.ImageUrl,
            request.CategoryId,
            request.VendorId,
            request.AvailableSizes,
            request.AvailableColors
        );

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(product.Id);
    }
}
