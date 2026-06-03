using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string Key { get; private set; }
    public string Value { get; private set; }

    private SystemSetting() { }

    public SystemSetting(string key, string value)
    {
        Id = Guid.NewGuid();
        Key = key;
        Value = value;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string value)
    {
        Value = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
