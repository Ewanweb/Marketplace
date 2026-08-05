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
    List<CreateOrderItemRequest> Items,
    Guid? UserId = null) : IRequest<Result<Guid>>;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShippingAddress).NotEmpty();
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

    public CreateOrderCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
            orderItems,
            request.UserId);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(order.Id);
    }
}
