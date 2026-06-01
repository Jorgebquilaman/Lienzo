namespace Lienzo.Application.DTOs.Common;

public record ErrorResponse(string Type, string Title, int Status, string Detail, string? Instance, Dictionary<string, string[]>? Errors);
