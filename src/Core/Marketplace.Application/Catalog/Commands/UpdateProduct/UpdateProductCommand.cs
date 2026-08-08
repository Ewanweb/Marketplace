using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Catalog.Commands.UpdateProduct;

using Marketplace.Application.Catalog.Commands.CreateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
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
    string AvailableSizes = "M,L",
    string AvailableColors = "Default",
    List<string>? ImageUrls = null,
    List<ProductAttributeDto>? Attributes = null) : IRequest<Result>;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TitleEn).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TitlePrs).NotEmpty().MaximumLength(250);
        RuleFor(x => x.TitlePs).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateProductCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(p => p.Images)
            .Include(p => p.Attributes)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null)
        {
            return Result.Failure(Error.NotFound("Product.NotFound", "Product not found."));
        }

        var targetVendorId = request.VendorId == Guid.Empty ? product.VendorId : request.VendorId;

        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure(Error.NotFound("Category.NotFound", "Category not found."));
        }

        var vendorExists = await _dbContext.Vendors
            .AnyAsync(v => v.Id == targetVendorId, cancellationToken);

        if (!vendorExists)
        {
            return Result.Failure(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        if (!_currentUserService.IsSuperAdmin)
        {
            var isMemberOfCurrentVendor = await _dbContext.VendorMembers
                .AnyAsync(vm => vm.VendorId == product.VendorId && vm.UserId == _currentUserService.UserId, cancellationToken);

            var isOwnerOfCurrentVendor = await _dbContext.Vendors
                .AnyAsync(v => v.Id == product.VendorId && v.UserId == _currentUserService.UserId && v.IsVerified, cancellationToken);

            if (!isMemberOfCurrentVendor && !isOwnerOfCurrentVendor)
            {
                return Result.Failure(Error.Forbidden("Vendor.Forbidden", "You do not have permission to modify this product."));
            }

            if (product.VendorId != targetVendorId)
            {
                var isMemberOfTargetVendor = await _dbContext.VendorMembers
                    .AnyAsync(vm => vm.VendorId == targetVendorId && vm.UserId == _currentUserService.UserId, cancellationToken);
                
                var isOwnerOfTargetVendor = await _dbContext.Vendors
                    .AnyAsync(v => v.Id == targetVendorId && v.UserId == _currentUserService.UserId && v.IsVerified, cancellationToken);

                if (!isMemberOfTargetVendor && !isOwnerOfTargetVendor)
                {
                    return Result.Failure(Error.Forbidden("Vendor.Forbidden", "You do not have permission to move this product to the target vendor."));
                }
            }
        }

        product.Update(
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
            targetVendorId,
            request.AvailableSizes,
            request.AvailableColors
        );

        if (request.ImageUrls != null && request.ImageUrls.Count > 0)
        {
            product.SetImages(request.ImageUrls);
        }
        else if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            product.SetImages(new[] { request.ImageUrl });
        }

        if (request.Attributes != null)
        {
            product.SetAttributes(request.Attributes.Select(a => (a.Key, a.Value)));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
