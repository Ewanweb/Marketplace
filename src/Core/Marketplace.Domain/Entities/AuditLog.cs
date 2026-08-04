namespace Marketplace.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(string action, string status, string ipAddress, string userAgent, Guid? userId = null, string? metadataJson = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(status);

        return new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            Status = status,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            MetadataJson = metadataJson,
            Timestamp = DateTime.UtcNow
        };
    }
}
