using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class KeyDelivery : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public Classroom Classroom { get; private set; } = null!;
    public Guid? DeliveredToUserId { get; private set; }
    public string DeliveredToName { get; private set; }
    public Guid DeliveredById { get; private set; }
    public DateTime DeliveredAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public string? Notes { get; private set; }

    private KeyDelivery() { }

    public KeyDelivery(Guid classroomId, Guid deliveredById, string deliveredToName, Guid? deliveredToUserId, string? notes)
    {
        Id = Guid.NewGuid();
        ClassroomId = classroomId;
        DeliveredById = deliveredById;
        DeliveredToName = deliveredToName;
        DeliveredToUserId = deliveredToUserId;
        Notes = notes;
        DeliveredAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Return()
    {
        ReturnedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Transfer(Guid newDeliveredById, string newDeliveredToName, Guid? newDeliveredToUserId, string? notes)
    {
        DeliveredById = newDeliveredById;
        DeliveredToName = newDeliveredToName;
        DeliveredToUserId = newDeliveredToUserId;
        Notes = notes;
        DeliveredAt = DateTime.UtcNow;
        ReturnedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
