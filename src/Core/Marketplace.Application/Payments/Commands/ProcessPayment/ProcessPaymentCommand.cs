using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Payments.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    Guid UserId,
    string PaymentMethod = "CreditCard") : IRequest<Result<Guid>>;

public sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}

public sealed class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _dbContext;

    public ProcessPaymentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _dbContext.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);
        if (order == null)
        {
            return Result.Failure<Guid>(Error.NotFound("Order.NotFound", "Order not found."));
        }

        var existingPayment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.Status == PaymentStatus.Success, cancellationToken);
        if (existingPayment != null)
        {
            return Result.Failure<Guid>(Error.Conflict("Payment.AlreadyPaid", "Order is already paid."));
        }

        var payment = Payment.Create(
            order.Id,
            request.UserId,
            order.TotalAmount,
            0.10m,
            request.PaymentMethod);

        payment.MarkAsSuccessful();
        order.UpdateStatus(OrderStatus.Processing);

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(payment.Id);
    }
}
