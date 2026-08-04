namespace Marketplace.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid VendorId { get; private set; }
    public string ProductTitle { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private OrderItem() { } // For EF Core

    public static OrderItem Create(Guid productId, Guid vendorId, string productTitle, decimal unitPrice, int quantity)
    {
        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            VendorId = vendorId,
            ProductTitle = productTitle,
            UnitPrice = unitPrice,
            Quantity = quantity
        };
    }
}
