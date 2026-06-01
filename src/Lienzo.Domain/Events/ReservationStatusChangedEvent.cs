using MediatR;

namespace Lienzo.Domain.Events;

public record ReservationStatusChangedEvent(
    Guid ReservationId,
    Guid ClassroomId,
    Guid UserId,
    string NewStatus,
    DateTime OccurredAt) : INotification;
