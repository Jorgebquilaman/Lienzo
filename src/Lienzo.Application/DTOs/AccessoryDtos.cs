namespace Lienzo.Application.DTOs;

public record AccessoryDto(Guid Id, string Name, string? Description, bool IsActive);

public record CreateAccessoryRequest(string Name, string? Description);

public record UpdateAccessoryRequest(string Name, string? Description, bool IsActive);
