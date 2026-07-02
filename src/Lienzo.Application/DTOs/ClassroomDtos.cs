namespace Lienzo.Application.DTOs;

public record ClassroomDto(Guid Id, string Name, string BuildingName, Guid BuildingId, int Floor, int Capacity, string Type, List<string> Features, bool IsActive, string? ImageUrl, float? MapPositionX, float? MapPositionY);

public record ClassroomDetailDto(Guid Id, string Name, string BuildingName, Guid BuildingId, int Floor, int Capacity, string Type, List<string> Features, bool IsActive, string? ImageUrl, DateTime CreatedAt, float? MapPositionX, float? MapPositionY);

public record CreateClassroomRequest(string Name, Guid BuildingId, int Floor, int Capacity, string Type, List<string>? Features, string? ImageUrl);

public record UpdateClassroomRequest(string? Name, int? Floor, int? Capacity, string? Type, List<string>? Features, string? ImageUrl);

public record ClassroomSummaryDto(Guid Id, string Name, int Capacity, string Type, string? ImageUrl, float? MapPositionX, float? MapPositionY);

public record AvailabilityResponse(Guid ClassroomId, DateTime Date, TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable, string? ConflictReason);
