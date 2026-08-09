using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Marketplace.Application.Common.Interfaces;

namespace Marketplace.Application.Catalog.Commands.CreateProduct;

public record ProductAttributeDto(string Key, string Value);

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
    string AvailableSizes = "M,L",
    string AvailableColors = "Default",
    List<string>? ImageUrls = null,
    List<ProductAttributeDto>? Attributes = null) : IRequest<Result<Guid>>;

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
    }
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMarketplaceEventPublisher _eventPublisher;

    public CreateProductCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IMarketplaceEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var targetVendorId = request.VendorId;
        if (targetVendorId == Guid.Empty)
        {
            var myVendorId = await _dbContext.Vendors
                .Where(v => v.UserId == _currentUserService.UserId && v.IsVerified)
                .Select(v => v.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (myVendorId == Guid.Empty)
            {
                myVendorId = await _dbContext.VendorMembers
                    .Where(vm => vm.UserId == _currentUserService.UserId)
                    .Select(vm => vm.VendorId)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            if (myVendorId == Guid.Empty)
            {
                return Result.Failure<Guid>(Error.NotFound("Vendor.NotFound", "Vendor profile not found for current user."));
            }

            targetVendorId = myVendorId;
        }

        var categoryExists = await _dbContext.Categories
            .AnyAsync(c => c.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "Category not found."));
        }

        var vendorExists = await _dbContext.Vendors
            .AnyAsync(v => v.Id == targetVendorId, cancellationToken);
            
        if (!vendorExists)
        {
            return Result.Failure<Guid>(Error.NotFound("Vendor.NotFound", "Vendor not found."));
        }

        if (!_currentUserService.IsSuperAdmin)
        {
            var isMember = await _dbContext.VendorMembers
                .AnyAsync(vm => vm.VendorId == targetVendorId && vm.UserId == _currentUserService.UserId, cancellationToken);
            
            if (!isMember)
            {
                var isOwner = await _dbContext.Vendors
                    .AnyAsync(v => v.Id == targetVendorId && v.UserId == _currentUserService.UserId && v.IsVerified, cancellationToken);

                if (!isOwner)
                {
                    return Result.Failure<Guid>(Error.Forbidden("Vendor.Forbidden", "You do not have permission to manage products for this vendor."));
                }
            }
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

        if (request.Attributes != null && request.Attributes.Count > 0)
        {
            product.SetAttributes(request.Attributes.Select(a => (a.Key, a.Value)));
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _eventPublisher.PublishProductAddedEvent(product.Id, cancellationToken);

        return Result.Success(product.Id);
    }
}
