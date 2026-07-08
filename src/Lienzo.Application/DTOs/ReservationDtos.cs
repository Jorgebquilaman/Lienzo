namespace Lienzo.Application.DTOs;

public class ReservationDto
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public string ClassroomName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? ApprovedById { get; set; }
    public Guid? RecurringGroupId { get; set; }
    public string? RecurrenceRule { get; set; }
    public Guid? ActividadId { get; set; }
    public string? ActividadNombre { get; set; }
    public string? ActividadPeriodo { get; set; }
    public string? ActividadCarrera { get; set; }
    public string? ActividadDocentes { get; set; }
    public List<string>? ActividadDocenteIds { get; set; }
}

public record CreateReservationRequest(Guid ClassroomId, string Title, string? Description, DateOnly Date, TimeOnly StartTime, TimeOnly EndTime, string? DaysOfWeek = null, DateOnly? EndDate = null, Guid? ActividadId = null);

public record UpdateReservationRequest(string? Title, string? Description);

public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages);

public record ScheduleResponse(
    List<ReservationDto> Reservations,
    List<MaintenanceBlockDto> MaintenanceBlocks);
