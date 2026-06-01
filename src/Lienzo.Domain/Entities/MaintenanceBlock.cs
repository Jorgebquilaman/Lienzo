using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class MaintenanceBlock : BaseEntity
{
    public Guid ClassroomId { get; private set; }
    public Classroom Classroom { get; private set; } = null!;
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string Reason { get; private set; }
    public Guid CreatedBy { get; private set; }
    public bool IsActive { get; private set; }

    private MaintenanceBlock() { }

    public MaintenanceBlock(Guid classroomId, DateTime startTime, DateTime endTime, string reason, Guid createdBy)
    {
        Id = Guid.NewGuid();
        ClassroomId = classroomId;
        StartTime = startTime;
        EndTime = endTime;
        Reason = reason;
        CreatedBy = createdBy;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
