namespace Lienzo.Application.DTOs;

public record BuildingDto(Guid Id, string Name, string Address, int FloorCount, bool IsActive, int? CodigoExterno, DateTime CreatedAt);

public record CreateBuildingRequest(string Name, string Address, int FloorCount);

public record UpdateBuildingRequest(string? Name, string? Address, int? FloorCount);

public record BuildingDetailDto(Guid Id, string Name, string Address, int FloorCount, bool IsActive, int? CodigoExterno, List<ClassroomSummaryDto> Classrooms, DateTime CreatedAt);
