using MediatR;

namespace Lienzo.Domain.Events;

public record AnnouncementCreatedEvent(
    Guid AnnouncementId,
    Guid TeacherId,
    string Title,
    string Body,
    string Type,
    DateTime OccurredAt) : INotification;
