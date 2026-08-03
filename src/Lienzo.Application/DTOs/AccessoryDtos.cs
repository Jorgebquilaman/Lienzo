namespace Lienzo.Application.DTOs;

public record AccessoryDto(Guid Id, string Name, string? Description, bool IsActive, bool IsMovable);

public record CreateAccessoryRequest(string Name, string? Description, bool IsMovable = false);

public record UpdateAccessoryRequest(string Name, string? Description, bool IsActive, bool IsMovable);
