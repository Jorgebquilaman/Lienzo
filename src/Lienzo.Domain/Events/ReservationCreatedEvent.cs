using MediatR;

namespace Lienzo.Domain.Events;

public record ReservationCreatedEvent(
    Guid ReservationId,
    Guid ClassroomId,
    Guid UserId,
    DateTime Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    DateTime OccurredAt) : INotification;
