using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public class ProcessedEmail : BaseEntity
{
    public string EmailUid { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid ProcessedByUserId { get; private set; }
    public DateTime ProcessedAt { get; private set; }

    private ProcessedEmail() { }

    public ProcessedEmail(string emailUid, Guid reservationId, Guid processedByUserId)
    {
        Id = Guid.NewGuid();
        EmailUid = emailUid;
        ReservationId = reservationId;
        ProcessedByUserId = processedByUserId;
        ProcessedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
