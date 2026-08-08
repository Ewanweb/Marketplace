namespace Marketplace.Domain.Entities;

public class ProductImage
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public string ImageUrl { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }

    private ProductImage() { }

    public static ProductImage Create(Guid productId, string imageUrl, int displayOrder = 0)
    {
        return new ProductImage
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            ImageUrl = imageUrl.Trim(),
            DisplayOrder = displayOrder
        };
    }

    public void Update(string imageUrl, int displayOrder)
    {
        ImageUrl = imageUrl.Trim();
        DisplayOrder = displayOrder;
    }
}
