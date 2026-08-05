using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Reports.Queries.GetOrderInvoice;

public sealed record InvoiceItemDto(
    string ProductTitle,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);

public sealed record OrderInvoiceDto(
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string ShippingAddress,
    string Status,
    DateTime CreatedAt,
    List<InvoiceItemDto> Items,
    decimal TotalAmount,
    string PaymentMethod,
    string TransactionId);

public sealed record GetOrderInvoiceQuery(Guid OrderId) : IRequest<Result<OrderInvoiceDto>>;

public sealed class GetOrderInvoiceQueryHandler : IRequestHandler<GetOrderInvoiceQuery, Result<OrderInvoiceDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetOrderInvoiceQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<OrderInvoiceDto>> Handle(GetOrderInvoiceQuery request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result.Failure<OrderInvoiceDto>(Error.NotFound("Order.NotFound", "Order not found."));
        }

        var payment = await _dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        var items = order.Items.Select(i => new InvoiceItemDto(
            i.Product != null ? i.Product.TitleEn : "Marketplace Item",
            i.Quantity,
            i.UnitPrice,
            i.Quantity * i.UnitPrice
        )).ToList();

        var invoice = new OrderInvoiceDto(
            order.Id,
            order.OrderNumber,
            order.CustomerName,
            order.ShippingAddress,
            order.Status.ToString(),
            order.CreatedAt,
            items,
            order.TotalAmount,
            payment != null ? payment.PaymentMethod : "Pending",
            payment != null ? payment.GatewayTransactionId : "N/A"
        );

        return Result.Success(invoice);
    }
}
