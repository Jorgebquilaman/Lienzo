namespace Lienzo.Application.DTOs;

public record AdminUserListItemDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    string? AvatarUrl,
    bool IsActive,
    DateTime CreatedAt);
