using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);

public sealed record CreateOrderCommand(
    List<CreateOrderItemRequest> Items,
    Guid UserId,
    string? ReferralCode = null) : IRequest<Result<Guid>>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
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

        var customsFeeSetting = await _dbContext.SiteSettings.FirstOrDefaultAsync(s => s.Key == "CustomsFeeAmount", cancellationToken);
        decimal customsFee = 0;
        if (customsFeeSetting != null && decimal.TryParse(customsFeeSetting.Value, out var parsedFee))
        {
            customsFee = parsedFee;
        }
        var noorzaiVendorId = Guid.Parse("66666666-6666-6666-6666-666666666666");

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
                var unitPrice = product.Price + (product.VendorId == noorzaiVendorId ? customsFee : 0);
                orderItems.Add(OrderItem.Create(
                    product.Id,
                    product.VendorId,
                    product.TitleEn,
                    unitPrice,
                    itemReq.Quantity));
            }
        }

        if (orderItems.Count == 0)
        {
            return Result.Failure<Guid>(Error.Validation("Order.NoValidProducts", "No valid products found for order."));
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<Guid>(Error.NotFound("User.NotFound", "User not found."));
        }

        if (string.IsNullOrWhiteSpace(user.FullName) || 
            string.IsNullOrWhiteSpace(user.PhoneNumber) || 
            string.IsNullOrWhiteSpace(user.Address))
        {
            return Result.Failure<Guid>(Error.Validation("Profile.Incomplete", Marketplace.Shared.Localization.LocalizedMessage.Get(
                "Please go to your profile and complete your Name, Address, and Phone Number before placing an order.",
                "لطفاً قبل از ثبت سفارش به پروفایل خود رفته و نام، آدرس و شماره تماس خود را تکمیل کنید.",
                "مهرباني وکړئ د فرمایش ورکولو دمخه خپل پروفایل ته لاړ شئ او خپل نوم، پته او د تلیفون شمیره بشپړه کړئ."
            )));
        }

        var order = Order.Create(
            user.FullName,
            user.Address,
            user.PhoneNumber,
            user.Email,
            orderItems,
            request.UserId,
            referrerUserId: null, // Will be set if referral code is valid
            referralCode: request.ReferralCode);

        // Process Affiliate Referral if present
        if (!string.IsNullOrWhiteSpace(request.ReferralCode))
        {
            var cleanRefCode = request.ReferralCode.Trim().ToLower();
            var referrer = await _dbContext.Users.FirstOrDefaultAsync(
                u => u.ReferralCode.ToLower() == cleanRefCode && u.Id != request.UserId, 
                cancellationToken);

            if (referrer != null)
            {
                order.SetReferrer(referrer.Id);

                // Fetch vendors to get their commission rates
                var vendorIds = orderItems.Select(i => i.VendorId).Distinct().ToList();
                var vendors = await _dbContext.Vendors
                    .Where(v => vendorIds.Contains(v.Id))
                    .ToDictionaryAsync(v => v.Id, cancellationToken);

                foreach (var item in orderItems)
                {
                    decimal commissionRate = 0.05m; // Default 5% referral commission
                    if (vendors.TryGetValue(item.VendorId, out var vendor) && vendor.AffiliateCommissionRate > 0)
                    {
                        commissionRate = vendor.AffiliateCommissionRate;
                    }

                    var referral = AffiliateReferral.Create(
                        referrer.Id,
                        order.Id,
                        item.Id,
                        item.VendorId,
                        item.ProductId,
                        item.TotalPrice,
                        commissionRate);
                    
                    referral.UpdateStatus(AffiliateStatus.Approved);
                    _dbContext.AffiliateReferrals.Add(referral);
                }
            }
        }

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        
        await _eventPublisher.PublishOrderUpdatedEvent(cancellationToken);

        return Result.Success(order.Id);
    }
}
