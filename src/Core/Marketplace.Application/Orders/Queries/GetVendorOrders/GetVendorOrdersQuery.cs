using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Orders.Queries.GetVendorOrders;

public sealed record VendorOrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductTitle,
    decimal UnitPrice,
    int Quantity,
    decimal TotalPrice,
    string Status);

public sealed record VendorOrderDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    string ShippingAddress,
    decimal TotalAmount,
    string Status,
    DateTime CreatedAt,
    List<VendorOrderItemDto> Items);

public sealed record GetVendorOrdersQuery(Guid VendorId) : IRequest<Result<List<VendorOrderDto>>>;

public sealed class GetVendorOrdersQueryHandler : IRequestHandler<GetVendorOrdersQuery, Result<List<VendorOrderDto>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetVendorOrdersQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<List<VendorOrderDto>>> Handle(GetVendorOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .AsQueryable();

        if (request.VendorId != Guid.Empty)
        {
            query = query.Where(o => o.Items.Any(i => i.VendorId == request.VendorId));
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = orders.Select(o => new VendorOrderDto(
            o.Id,
            o.OrderNumber,
            o.CustomerName,
            o.ShippingAddress,
            o.TotalAmount,
            o.Status.ToString(),
            o.CreatedAt,
            o.Items
                .Where(i => request.VendorId == Guid.Empty || i.VendorId == request.VendorId)
                .Select(i => new VendorOrderItemDto(
                    i.Id,
                    i.ProductId,
                    i.ProductTitle,
                    i.UnitPrice,
                    i.Quantity,
                    i.TotalPrice,
                    i.Status.ToString()))
                .ToList()
        )).ToList();

        return Result.Success(result);
    }
}
