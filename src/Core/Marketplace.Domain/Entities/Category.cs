namespace Marketplace.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }
    public string NameEn { get; private set; } = string.Empty;
    public string NamePrs { get; private set; } = string.Empty;
    public string NamePs { get; private set; } = string.Empty;
    public string IconName { get; private set; } = string.Empty;
    public Guid? ParentId { get; private set; }
    public int Level { get; private set; } = 1;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Category? Parent { get; private set; }
    public ICollection<Category> SubCategories { get; private set; } = new List<Category>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();

    private Category() { } // For EF Core

    public static Category Create(
        string nameEn,
        string namePrs,
        string namePs,
        string iconName,
        Guid? parentId = null,
        int level = 1)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            NameEn = nameEn.Trim(),
            NamePrs = namePrs.Trim(),
            NamePs = namePs.Trim(),
            IconName = iconName.Trim(),
            ParentId = parentId,
            Level = level,
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
