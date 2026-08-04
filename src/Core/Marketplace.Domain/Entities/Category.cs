namespace Marketplace.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string NameEn { get; private set; } = string.Empty;
    public string NamePrs { get; private set; } = string.Empty;
    public string NamePs { get; private set; } = string.Empty;
    public string IconName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { } // For EF Core

    public static Category Create(
        string nameEn,
        string namePrs,
        string namePs,
        string iconName)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            NameEn = nameEn.Trim(),
            NamePrs = namePrs.Trim(),
            NamePs = namePs.Trim(),
            IconName = iconName.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public string GetName(string cultureName)
    {
        var culture = cultureName.ToLowerInvariant();
        if (culture.StartsWith("ps")) return NamePs;
        if (culture.StartsWith("prs") || culture.StartsWith("fa")) return NamePrs;
        return NameEn;
    }
}
