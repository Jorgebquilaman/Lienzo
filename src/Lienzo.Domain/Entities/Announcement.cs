using Lienzo.Domain.Common;
using Lienzo.Domain.Enums;
using Lienzo.Domain.Events;

namespace Lienzo.Domain.Entities;

public class Announcement : BaseEntity
{
    private readonly List<AnnouncementRecipient> _recipients = [];

    public Guid TeacherId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public AnnouncementType Type { get; private set; }
    public TargetAudience TargetAudience { get; private set; }
    public Guid? RelatedReservationId { get; private set; }
    public Reservation? RelatedReservation { get; private set; }
    public IReadOnlyCollection<AnnouncementRecipient> Recipients => _recipients.AsReadOnly();

    private Announcement() { }

    public Announcement(
        Guid teacherId,
        string title,
        string body,
        AnnouncementType type,
        TargetAudience targetAudience,
        Guid? relatedReservationId = null)
    {
        Id = Guid.NewGuid();
        TeacherId = teacherId;
        SetTitle(title);
        SetBody(body);
        Type = type;
        TargetAudience = targetAudience;
        RelatedReservationId = relatedReservationId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new AnnouncementCreatedEvent(
            Id,
            teacherId,
            title,
            body,
            type.ToString(),
            DateTime.UtcNow));
    }

    public void AddRecipient(Guid studentId)
    {
        if (_recipients.Any(r => r.StudentId == studentId))
            return;

        _recipients.Add(new AnnouncementRecipient(Id, studentId));
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead(Guid studentId)
    {
        var recipient = _recipients.FirstOrDefault(r => r.StudentId == studentId);
        if (recipient is not null)
        {
            recipient.MarkAsRead();
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void AddRecipients(IEnumerable<Guid> studentIds)
    {
        foreach (var studentId in studentIds)
            AddRecipient(studentId);
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
