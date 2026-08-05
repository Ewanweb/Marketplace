namespace Marketplace.Domain.Entities;

public enum PaymentStatus
{
    Pending = 1,
    Success = 2,
    Failed = 3,
    Refunded = 4
}

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public decimal Amount { get; private set; }
    public decimal PlatformFee { get; private set; }
    public decimal VendorAmount { get; private set; }
    
    public string PaymentMethod { get; private set; } = "CreditCard";
    public string GatewayTransactionId { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId,
        Guid userId,
        decimal amount,
        decimal commissionRate = 0.10m,
        string paymentMethod = "CreditCard")
    {
        var platformFee = Math.Round(amount * commissionRate, 2);
        var vendorAmount = amount - platformFee;

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = userId,
            Amount = amount,
            PlatformFee = platformFee,
            VendorAmount = vendorAmount,
            PaymentMethod = paymentMethod,
            GatewayTransactionId = $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}",
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsSuccessful()
    {
        Status = PaymentStatus.Success;
        PaidAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = PaymentStatus.Failed;
    }
}
