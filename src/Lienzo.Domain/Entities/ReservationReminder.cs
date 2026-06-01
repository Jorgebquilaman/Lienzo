using Lienzo.Domain.Common;

namespace Lienzo.Domain.Entities;

public enum ReminderType
{
    TwentyFourHours,
    ThirtyMinutes
}

public class ReservationReminder : BaseEntity
{
    public Guid ReservationId { get; private set; }
    public Guid UserId { get; private set; }
    public ReminderType ReminderType { get; private set; }
    public DateTime SentAt { get; private set; }

    private ReservationReminder() { }

    public ReservationReminder(Guid reservationId, Guid userId, ReminderType reminderType)
    {
        Id = Guid.NewGuid();
        ReservationId = reservationId;
        UserId = userId;
        ReminderType = reminderType;
        SentAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
