namespace Lienzo.Application.DTOs;

public record RecesoDto(Guid Id, string StartDate, string EndDate, string Description);

public record CreateRecesoRequest(string StartDate, string EndDate, string Description);
