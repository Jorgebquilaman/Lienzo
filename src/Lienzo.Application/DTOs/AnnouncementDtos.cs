namespace Lienzo.Application.DTOs;

public class AnnouncementDto
{
    public Guid Id { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TargetAudience { get; set; } = string.Empty;
    public Guid? RelatedReservationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}

public record CreateAnnouncementRequest(string Title, string Body, string Type, string TargetAudience, Guid? RelatedReservationId, List<Guid>? SpecificStudentIds);

public class AnnouncementListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? UserName { get; set; }
}
