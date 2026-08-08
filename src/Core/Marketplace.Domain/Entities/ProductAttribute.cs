namespace Marketplace.Domain.Entities;

public class ProductAttribute
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;

    private ProductAttribute() { }

    public static ProductAttribute Create(Guid productId, string key, string value)
    {
        return new ProductAttribute
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Key = key.Trim(),
            Value = value.Trim()
        };
    }

    public void Update(string key, string value)
    {
        Key = key.Trim();
        Value = value.Trim();
    }
}
