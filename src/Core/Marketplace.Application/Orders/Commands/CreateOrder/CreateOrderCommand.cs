using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(
    string CustomerName,
    string ShippingAddress,
    string Phone,
    string Email,
    List<CreateOrderItemRequest> Items,
    Guid? UserId = null,
    string? ReferralCode = null) : IRequest<Result<Guid>>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShippingAddress).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(100);
        });
    }
}

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMarketplaceEventPublisher _eventPublisher;

    public CreateOrderCommandHandler(IApplicationDbContext dbContext, IMarketplaceEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _dbContext.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var orderItems = new List<OrderItem>();
        foreach (var itemReq in request.Items)
        {
            if (products.TryGetValue(itemReq.ProductId, out var product))
            {
                if (!product.HasSufficientStock(itemReq.Quantity))
                {
                    return Result.Failure<Guid>(Error.Validation("Product.InsufficientStock",
                        $"Product '{product.TitleEn}' has only {product.StockQuantity} items in stock."));
                }

                product.DecreaseStock(itemReq.Quantity);
                orderItems.Add(OrderItem.Create(
                    product.Id,
                    product.VendorId,
                    product.TitleEn,
                    product.Price,
                    itemReq.Quantity));
            }
        }

        if (orderItems.Count == 0)
        {
            return Result.Failure<Guid>(Error.Validation("Order.NoValidProducts", "No valid products found for order."));
        }

        var order = Order.Create(
            request.CustomerName,
            request.ShippingAddress,
            request.Phone,
            request.Email,
            orderItems,
            request.UserId,
            referrerUserId: null, // Will be set if referral code is valid
            referralCode: request.ReferralCode);

        // Process Affiliate Referral if present
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var referrer = await _dbContext.Users.FirstOrDefaultAsync(u => u.ReferralCode == request.ReferralCode && u.Id != request.UserId, cancellationToken);
            if (referrer != null)
            {
                var orderTypeType = order.GetType();
                var referrerProperty = orderTypeType.GetProperty("ReferrerUserId");
                if (referrerProperty != null && referrerProperty.CanWrite)
                {
                    referrerProperty.SetValue(order, referrer.Id);
                }
                else
                {
                    // Update order entity via reflection if property is private set
                    var field = orderTypeType.GetProperty("ReferrerUserId", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.DeclaringType?.GetProperty("ReferrerUserId")?.SetValue(order, referrer.Id);
                    }
                }
                
                // Fetch vendors to get their commission rates
                var vendorIds = orderItems.Select(i => i.VendorId).Distinct().ToList();
                var vendors = await _dbContext.Vendors.Where(v => vendorIds.Contains(v.Id)).ToDictionaryAsync(v => v.Id, cancellationToken);

                foreach (var item in orderItems)
                {
                    if (vendors.TryGetValue(item.VendorId, out var vendor) && vendor.AffiliateCommissionRate > 0)
                    {
                        var referral = AffiliateReferral.Create(
                            referrer.Id,
                            order.Id,
                            item.Id,
                            item.VendorId,
                            item.ProductId,
                            item.TotalPrice,
                            vendor.AffiliateCommissionRate);
                        
                        _dbContext.AffiliateReferrals.Add(referral);
                    }
                }
            }
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        
        await _eventPublisher.PublishOrderUpdatedEvent(cancellationToken);

        return Result.Success(order.Id);
    }
}
