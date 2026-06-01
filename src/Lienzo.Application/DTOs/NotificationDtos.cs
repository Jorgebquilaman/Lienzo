namespace Lienzo.Application.DTOs;

public record NotificationDto(Guid Id, string Title, string Body, string Type, bool IsRead, DateTime CreatedAt, Guid? RelatedEntityId, string? RelatedEntityType);
