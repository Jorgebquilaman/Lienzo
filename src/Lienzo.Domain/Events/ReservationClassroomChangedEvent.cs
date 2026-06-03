using MediatR;

namespace Lienzo.Domain.Events;

public record ReservationClassroomChangedEvent(
    Guid ReservationId,
    Guid OldClassroomId,
    Guid NewClassroomId,
    Guid UserId,
    DateTime OccurredAt) : INotification;
