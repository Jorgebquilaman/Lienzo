namespace Lienzo.Domain.Entities;

public class AnnouncementRecipient
{
    public Guid AnnouncementId { get; private set; }
    public Announcement Announcement { get; private set; } = null!;
    public Guid StudentId { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private AnnouncementRecipient() { }

    public AnnouncementRecipient(Guid announcementId, Guid studentId)
    {
        AnnouncementId = announcementId;
        StudentId = studentId;
        ReadAt = null;
    }

    public void MarkAsRead()
    {
        if (ReadAt.HasValue) return;
        ReadAt = DateTime.UtcNow;
    }

    public bool IsRead => ReadAt.HasValue;
}
