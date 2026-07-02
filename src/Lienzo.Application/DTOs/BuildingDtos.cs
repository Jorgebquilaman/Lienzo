namespace Lienzo.Application.DTOs;

public record BuildingDto(Guid Id, string Name, string Address, int FloorCount, bool IsActive, int? CodigoExterno, DateTime CreatedAt, string? FloorPlanUrl);

public record CreateBuildingRequest(string Name, string Address, int FloorCount);

public record UpdateBuildingRequest(string? Name, string? Address, int? FloorCount);

public record BuildingDetailDto(Guid Id, string Name, string Address, int FloorCount, bool IsActive, int? CodigoExterno, List<ClassroomSummaryDto> Classrooms, DateTime CreatedAt, string? FloorPlanUrl);

public record SetBuildingFloorPlanRequest(string? FloorPlanUrl);

public record UpdateClassroomPositionsRequest(List<ClassroomPositionDto> Positions);

public record ClassroomPositionDto(Guid ClassroomId, float? X, float? Y);
