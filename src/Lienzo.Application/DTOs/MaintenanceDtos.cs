namespace Lienzo.Application.DTOs;

public record MaintenanceBlockDto(
    Guid Id,
    Guid ClassroomId,
    string ClassroomName,
    string? BuildingName,
    DateTime StartTime,
    DateTime EndTime,
    string Reason,
    string CreatedBy,
    DateTime CreatedAt);

public record CreateMaintenanceBlockRequest(
    Guid ClassroomId,
    DateTime StartTime,
    DateTime EndTime,
    string Reason);

public record MaintenanceBlockListResponse(
    List<MaintenanceBlockDto> Items,
    int TotalCount);
