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
    private readonly IMarketplaceEventPublisher _eventPublisher;

    public UpdateOrderItemStatusCommandHandler(IApplicationDbContext dbContext, IMarketplaceEventPublisher eventPublisher)
    {
        _dbContext = dbContext;
        _eventPublisher = eventPublisher;
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

        var activeItems = allOrderItems.Where(oi => oi.Status != OrderStatus.Cancelled).ToList();

        if (allOrderItems.All(oi => oi.Status == OrderStatus.Cancelled))
        {
            orderItem.Order.UpdateStatus(OrderStatus.Cancelled);
        }
        else if (activeItems.All(oi => oi.Status == OrderStatus.Delivered))
        {
            orderItem.Order.UpdateStatus(OrderStatus.Delivered);
        }
        else if (activeItems.All(oi => oi.Status == OrderStatus.Shipped || oi.Status == OrderStatus.Delivered))
        {
            orderItem.Order.UpdateStatus(OrderStatus.Shipped);
        }
        else if (activeItems.All(oi => oi.Status == OrderStatus.Pending))
        {
            orderItem.Order.UpdateStatus(OrderStatus.Pending);
        }
        else
        {
            orderItem.Order.UpdateStatus(OrderStatus.Processing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        await _eventPublisher.PublishOrderUpdatedEvent(cancellationToken);

        return Result.Success();
    }
}
