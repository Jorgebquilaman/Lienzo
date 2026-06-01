using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Holiday : BaseEntity
{
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = string.Empty;

    private Holiday() { }

    public Holiday(DateOnly date, string description)
    {
        Id = Guid.NewGuid();
        Date = date;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
