namespace Marketplace.Domain.Entities;

public class Vendor
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public string ShopNameEn { get; private set; } = string.Empty;
    public string ShopNamePrs { get; private set; } = string.Empty;
    public string ShopNamePs { get; private set; } = string.Empty;
    
    public string DescriptionEn { get; private set; } = string.Empty;
    public string DescriptionPrs { get; private set; } = string.Empty;
    public string DescriptionPs { get; private set; } = string.Empty;

    public string LogoUrl { get; private set; } = string.Empty;
    public string BannerUrl { get; private set; } = string.Empty;
    
    public decimal CommissionRate { get; private set; } // e.g., 0.10 for 10%
    public bool IsVerified { get; private set; }
    public double Rating { get; private set; }
    
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Vendor() { } // For EF Core

    public static Vendor Create(
        Guid userId,
        string shopNameEn,
        string shopNamePrs,
        string shopNamePs,
        string descriptionEn,
        string descriptionPrs,
        string descriptionPs,
        string logoUrl = "",
        string bannerUrl = "",
        decimal commissionRate = 0.10m)
    {
        return new Vendor
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShopNameEn = shopNameEn.Trim(),
            ShopNamePrs = shopNamePrs.Trim(),
            ShopNamePs = shopNamePs.Trim(),
            DescriptionEn = descriptionEn.Trim(),
            DescriptionPrs = descriptionPrs.Trim(),
            DescriptionPs = descriptionPs.Trim(),
            LogoUrl = logoUrl,
            BannerUrl = bannerUrl,
            CommissionRate = commissionRate,
            IsVerified = false,
            Rating = 5.0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Verify()
    {
        IsVerified = true;
    }

    public void UpdateCommissionRate(decimal newRate)
    {
        if (newRate >= 0 && newRate <= 1)
        {
            CommissionRate = newRate;
        }
    }

    public string GetShopName(string cultureName)
    {
        var culture = cultureName.ToLowerInvariant();
        if (culture.StartsWith("ps")) return ShopNamePs;
        if (culture.StartsWith("prs") || culture.StartsWith("fa")) return ShopNamePrs;
        return ShopNameEn;
    }

    public string GetDescription(string cultureName)
    {
        var culture = cultureName.ToLowerInvariant();
        if (culture.StartsWith("ps")) return DescriptionPs;
        if (culture.StartsWith("prs") || culture.StartsWith("fa")) return DescriptionPrs;
        return DescriptionEn;
    }
}
