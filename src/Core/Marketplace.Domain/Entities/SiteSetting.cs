namespace Marketplace.Domain.Entities;

public class SiteSetting
{
    public Guid Id { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    private SiteSetting() { } // For EF Core

    public static SiteSetting Create(string key, string value)
    {
        return new SiteSetting
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            Value = value.Trim(),
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateValue(string value)
    {
        Value = value.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
