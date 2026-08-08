using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Orders.Commands.UpdateOrderItemStatus;

public sealed record UpdateOrderItemStatusCommand(
    Guid OrderItemId,
    Guid VendorId,
    OrderStatus NewStatus) : IRequest<Result>;

public sealed class UpdateOrderItemStatusCommandValidator : AbstractValidator<UpdateOrderItemStatusCommand>
{
    public UpdateOrderItemStatusCommandValidator()
    {
        RuleFor(x => x.OrderItemId).NotEmpty();
        RuleFor(x => x.VendorId).NotEmpty();
        RuleFor(x => x.NewStatus).IsInEnum();
    }
}

public sealed class UpdateOrderItemStatusCommandHandler : IRequestHandler<UpdateOrderItemStatusCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateOrderItemStatusCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateOrderItemStatusCommand request, CancellationToken cancellationToken)
    {
        var orderItem = await _dbContext.OrderItems
            .Include(oi => oi.Order)
            .FirstOrDefaultAsync(oi => oi.Id == request.OrderItemId, cancellationToken);

        if (orderItem == null)
        {
            return Result.Failure(Error.NotFound("OrderItem.NotFound", "Order item not found."));
        }

        if (orderItem.VendorId != request.VendorId)
        {
            return Result.Failure(Error.Forbidden("OrderItem.Forbidden", "You do not have permission to update this order item."));
        }

        orderItem.UpdateStatus(request.NewStatus);

        // Optional: If all items in the order are shipped/delivered, update the order status
        var allOrderItems = await _dbContext.OrderItems
            .Where(oi => oi.OrderId == orderItem.OrderId)
            .ToListAsync(cancellationToken);

        var allShipped = allOrderItems.All(oi => oi.Status == OrderStatus.Shipped || oi.Status == OrderStatus.Delivered);
        var allDelivered = allOrderItems.All(oi => oi.Status == OrderStatus.Delivered);

        if (allDelivered)
        {
            orderItem.Order.UpdateStatus(OrderStatus.Delivered);
        }
        else if (allShipped)
        {
            orderItem.Order.UpdateStatus(OrderStatus.Shipped);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
