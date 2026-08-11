using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Orders.Queries.GetMyOrders;

public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductTitle,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice);

public sealed record OrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string ShippingAddress,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    List<OrderItemDto> Items);

public sealed record GetMyOrdersQuery(Guid UserId) : IRequest<Result<List<OrderDto>>>;

public sealed class GetMyOrdersQueryHandler : IRequestHandler<GetMyOrdersQuery, Result<List<OrderDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetMyOrdersQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<OrderDto>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderDto(
                o.Id,
                o.OrderNumber,
                o.CustomerName,
                o.ShippingAddress,
                o.TotalAmount,
                o.Status.ToString(),
                o.CreatedAt,
                o.Items.Select(i => new OrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductTitle,
                    i.UnitPrice,
                    i.Quantity,
                    i.UnitPrice * i.Quantity)).ToList()))
            .ToListAsync(cancellationToken);

        return Result.Success(orders);
    }
}
