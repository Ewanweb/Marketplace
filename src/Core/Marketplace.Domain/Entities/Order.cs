namespace Marketplace.Domain.Entities;

public enum OrderStatus
{
    Pending = 0,
    Processing = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}

public class Order
{
    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public ICollection<OrderItem> Items { get; private set; } = new List<OrderItem>();

    private Order() { } // For EF Core

    public static Order Create(
        string customerName,
        string shippingAddress,
        string phone,
        string email,
        List<OrderItem> items,
        Guid? userId = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            UserId = userId,
            CustomerName = customerName.Trim(),
            ShippingAddress = shippingAddress.Trim(),
            Phone = phone?.Trim() ?? string.Empty,
            Email = email?.Trim() ?? string.Empty,
            Status = OrderStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            Items = items
        };

        order.TotalAmount = items.Sum(i => i.TotalPrice);
        return order;
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
    }
}
