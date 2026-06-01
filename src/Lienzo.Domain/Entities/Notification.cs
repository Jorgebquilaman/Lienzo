using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;

namespace Lienzo.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    private Notification() { }

    public Notification(
        Guid userId,
        string title,
        string body,
        NotificationType type = NotificationType.Info,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        SetTitle(title);
        SetBody(body);
        Type = type;
        IsRead = false;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        UpdatedAt = DateTime.UtcNow;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        Title = title;
    }

    private void SetBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be empty.", nameof(body));
        Body = body;
    }
}
