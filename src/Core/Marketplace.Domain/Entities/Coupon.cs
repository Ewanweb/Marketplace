namespace Marketplace.Domain.Entities;

public class Coupon
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public decimal DiscountPercent { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public bool IsPercentage { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public decimal MaxDiscountAmount { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public int UsageLimit { get; private set; }
    public int UsedCount { get; private set; }
    public bool IsActive { get; private set; }
    public Guid? VendorId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Coupon() { }

    public static Coupon Create(
        string code,
        decimal discountPercent,
        decimal discountAmount,
        bool isPercentage,
        decimal minOrderAmount = 0m,
        decimal maxDiscountAmount = 1000m,
        DateTime? expiryDate = null,
        int usageLimit = 1000,
        Guid? vendorId = null)
    {
        return new Coupon
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            DiscountPercent = discountPercent,
            DiscountAmount = discountAmount,
            IsPercentage = isPercentage,
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            ExpiryDate = expiryDate ?? DateTime.UtcNow.AddMonths(1),
            UsageLimit = usageLimit,
            UsedCount = 0,
            IsActive = true,
            VendorId = vendorId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public bool IsValid(decimal orderAmount, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!IsActive)
        {
            errorMessage = "Coupon is inactive.";
            return false;
        }

        if (DateTime.UtcNow > ExpiryDate)
        {
            errorMessage = "Coupon has expired.";
            return false;
        }

        if (UsedCount >= UsageLimit)
        {
            errorMessage = "Coupon usage limit reached.";
            return false;
        }

        if (orderAmount < MinOrderAmount)
        {
            errorMessage = $"Minimum order amount for this coupon is ${MinOrderAmount:F2}.";
            return false;
        }

        return true;
    }

    public decimal CalculateDiscount(decimal orderAmount)
    {
        if (orderAmount < MinOrderAmount) return 0m;

        decimal calculated = IsPercentage
            ? Math.Round(orderAmount * (DiscountPercent / 100m), 2)
            : DiscountAmount;

        return Math.Min(calculated, MaxDiscountAmount);
    }

    public void IncrementUsage()
    {
        UsedCount++;
    }
}
