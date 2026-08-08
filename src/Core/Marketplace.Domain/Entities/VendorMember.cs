namespace Marketplace.Domain.Entities;

public enum VendorRole
{
    Owner,
    Staff
}

public class VendorMember
{
    public Guid Id { get; private set; }
    public Guid VendorId { get; private set; }
    public Vendor Vendor { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public VendorRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private VendorMember() { } // For EF Core

    public static VendorMember Create(Guid vendorId, Guid userId, VendorRole role)
    {
        return new VendorMember
        {
            Id = Guid.NewGuid(),
            VendorId = vendorId,
            UserId = userId,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }
}
