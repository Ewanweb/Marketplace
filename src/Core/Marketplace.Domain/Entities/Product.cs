namespace Marketplace.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string TitleEn { get; private set; } = string.Empty;
    public string TitlePrs { get; private set; } = string.Empty;
    public string TitlePs { get; private set; } = string.Empty;
    public string DescriptionEn { get; private set; } = string.Empty;
    public string DescriptionPrs { get; private set; } = string.Empty;
    public string DescriptionPs { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public double Rating { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    
    public Guid VendorId { get; private set; }
    public Vendor Vendor { get; private set; } = null!;
    
    public string AvailableSizes { get; private set; } = "M,L";
    public string AvailableColors { get; private set; } = "Default";
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<ProductImage> _images = new();
    public IReadOnlyCollection<ProductImage> Images => _images.AsReadOnly();

    private readonly List<ProductAttribute> _attributes = new();
    public IReadOnlyCollection<ProductAttribute> Attributes => _attributes.AsReadOnly();

    private Product() { } // For EF Core

    public static Product Create(
        string titleEn,
        string titlePrs,
        string titlePs,
        string descriptionEn,
        string descriptionPrs,
        string descriptionPs,
        decimal price,
        int stockQuantity,
        string imageUrl,
        Guid categoryId,
        Guid vendorId,
        string availableSizes = "M,L",
        string availableColors = "Default",
        double rating = 5.0)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            TitleEn = titleEn.Trim(),
            TitlePrs = titlePrs.Trim(),
            TitlePs = titlePs.Trim(),
            DescriptionEn = descriptionEn.Trim(),
            DescriptionPrs = descriptionPrs.Trim(),
            DescriptionPs = descriptionPs.Trim(),
            Price = price,
            StockQuantity = stockQuantity,
            ImageUrl = imageUrl,
            CategoryId = categoryId,
            VendorId = vendorId,
            AvailableSizes = availableSizes,
            AvailableColors = availableColors,
            Rating = rating,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string titleEn,
        string titlePrs,
        string titlePs,
        string descriptionEn,
        string descriptionPrs,
        string descriptionPs,
        decimal price,
        int stockQuantity,
        string imageUrl,
        Guid categoryId,
        Guid vendorId,
        string availableSizes = "M,L",
        string availableColors = "Default")
    {
        TitleEn = titleEn.Trim();
        TitlePrs = titlePrs.Trim();
        TitlePs = titlePs.Trim();
        DescriptionEn = descriptionEn.Trim();
        DescriptionPrs = descriptionPrs.Trim();
        DescriptionPs = descriptionPs.Trim();
        Price = price;
        StockQuantity = stockQuantity;
        ImageUrl = imageUrl;
        CategoryId = categoryId;
        VendorId = vendorId;
        AvailableSizes = availableSizes;
        AvailableColors = availableColors;
    }

    public bool HasSufficientStock(int quantity) => StockQuantity >= quantity;

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        if (StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock. Requested: {quantity}, Available: {StockQuantity}");

        StockQuantity -= quantity;
    }

    public void SoftDelete()
    {
        IsActive = false;
    }

    public void UpdateRating(double newRating)
    {
        if (newRating >= 1.0 && newRating <= 5.0)
        {
            Rating = Math.Round(newRating, 1);
        }
    }

    public string GetTitle(string cultureName)
    {
        var culture = cultureName.ToLowerInvariant();
        if (culture.StartsWith("ps")) return TitlePs;
        if (culture.StartsWith("prs") || culture.StartsWith("fa")) return TitlePrs;
        return TitleEn;
    }

    public string GetDescription(string cultureName)
    {
        var culture = cultureName.ToLowerInvariant();
        if (culture.StartsWith("ps")) return DescriptionPs;
        if (culture.StartsWith("prs") || culture.StartsWith("fa")) return DescriptionPrs;
        return DescriptionEn;
    }

    public void SetImages(IEnumerable<string>? imageUrls)
    {
        var urls = (imageUrls ?? Enumerable.Empty<string>())
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .ToList();

        _images.Clear();

        for (int i = 0; i < urls.Count; i++)
        {
            _images.Add(ProductImage.Create(Id, urls[i], i));
        }

        if (_images.Any())
        {
            ImageUrl = _images.First().ImageUrl;
        }
    }

    public void SetAttributes(IEnumerable<(string Key, string Value)>? attributes)
    {
        var attrs = (attributes ?? Enumerable.Empty<(string Key, string Value)>())
            .Where(a => !string.IsNullOrWhiteSpace(a.Key) && !string.IsNullOrWhiteSpace(a.Value))
            .ToList();

        _attributes.Clear();

        foreach (var attr in attrs)
        {
            _attributes.Add(ProductAttribute.Create(Id, attr.Key, attr.Value));
        }
    }
}
