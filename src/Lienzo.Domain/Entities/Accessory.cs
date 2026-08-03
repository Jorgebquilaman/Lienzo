using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class Accessory : BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsMovable { get; private set; }

    private Accessory() { }

    public Accessory(string name, string? description, bool isMovable = false)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        IsMovable = isMovable;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, bool isActive, bool isMovable)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        IsMovable = isMovable;
        UpdatedAt = DateTime.UtcNow;
    }
}
