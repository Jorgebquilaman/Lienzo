using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class ClassroomAccessory : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public Classroom Classroom { get; private set; } = null!;
    public Guid AccessoryId { get; private set; }
    public Accessory Accessory { get; private set; } = null!;

    private ClassroomAccessory() { }

    public ClassroomAccessory(Guid classroomId, Guid accessoryId)
    {
        Id = Guid.NewGuid();
        ClassroomId = classroomId;
        AccessoryId = accessoryId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
