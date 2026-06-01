using MediatR;

namespace Lienzo.Domain.Events;

public record ReservationUpdatedEvent(
    Guid ReservationId,
    Guid UserId,
    string PreviousTitle,
    string? PreviousDescription,
    DateTime OccurredAt) : INotification;
