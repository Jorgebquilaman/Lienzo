namespace Lienzo.Application.DTOs;

public class ReservationPdfModel
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string ClassroomName { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public int Floor { get; set; }
    public string Date { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid ReservationId { get; set; }
    public List<string> Accessories { get; set; } = [];
}

public record EmailAttachment(string FileName, string ContentType, byte[] Content);