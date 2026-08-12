namespace Marketplace.Domain.Entities;

public class AffiliateReferral
{
    public Guid Id { get; private set; }
    public Guid ReferrerUserId { get; private set; }
    public User Referrer { get; private set; } = null!;
    
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    
    public Guid OrderItemId { get; private set; }
    public OrderItem OrderItem { get; private set; } = null!;
    
    public Guid VendorId { get; private set; }
    public Vendor Vendor { get; private set; } = null!;
    
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public decimal OrderItemTotal { get; private set; }
    public decimal CommissionRate { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public AffiliateStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private AffiliateReferral() { }

    public static AffiliateReferral Create(
        Guid referrerUserId,
        Guid orderId,
        Guid orderItemId,
        Guid vendorId,
        Guid productId,
        decimal orderItemTotal,
        decimal commissionRate)
    {
        return new AffiliateReferral
        {
            Id = Guid.NewGuid(),
            ReferrerUserId = referrerUserId,
            OrderId = orderId,
            OrderItemId = orderItemId,
            VendorId = vendorId,
            ProductId = productId,
            OrderItemTotal = orderItemTotal,
            CommissionRate = commissionRate,
            CommissionAmount = orderItemTotal * commissionRate,
            Status = AffiliateStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(AffiliateStatus status)
    {
        Status = status;
        if (status == AffiliateStatus.Paid)
        {
            PaidAt = DateTime.UtcNow;
        }
    }
}
