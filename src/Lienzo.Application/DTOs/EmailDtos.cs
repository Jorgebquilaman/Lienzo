namespace Lienzo.Application.DTOs;

public record EmailAttachmentDto(string Name, string ContentType, long Size);

public record EmailMessageSummaryDto(
    string Uid,
    string From,
    string FromName,
    string Subject,
    DateTimeOffset Date,
    bool HasAttachment,
    bool IsRead,
    bool IsProcessed,
    string? Snippet);

public record EmailMessageDetailDto(
    string Uid,
    string From,
    string FromName,
    string Subject,
    DateTimeOffset Date,
    bool HasAttachment,
    bool IsRead,
    bool IsProcessed,
    string? BodyText,
    string? BodyHtml,
    List<EmailAttachmentDto> Attachments,
    Guid? ReservationId,
    bool RequiresAccessoryConfirmation,
    bool AccessoriesConfirmed);

public record EmailProcessedInfoDto(
    string EmailUid,
    Guid? ReservationId,
    string? ReservationTitle,
    DateTime? ProcessedAt,
    string? ProcessedByName);
