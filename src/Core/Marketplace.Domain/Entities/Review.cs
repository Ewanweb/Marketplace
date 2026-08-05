namespace Marketplace.Domain.Entities;

public class Review
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;

    public Guid VendorId { get; private set; }

    public int Rating { get; private set; } // 1 to 5 stars
    public string Comment { get; private set; } = string.Empty;
    public bool IsVerifiedPurchase { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Review() { }

    public static Review Create(
        Guid userId,
        Guid productId,
        Guid vendorId,
        int rating,
        string comment,
        bool isVerifiedPurchase = false)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5 stars.");
        }

        return new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProductId = productId,
            VendorId = vendorId,
            Rating = rating,
            Comment = (comment ?? string.Empty).Trim(),
            IsVerifiedPurchase = isVerifiedPurchase,
            CreatedAt = DateTime.UtcNow
        };
    }
}
