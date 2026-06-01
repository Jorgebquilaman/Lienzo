namespace Lienzo.Application.DTOs;

public record CarreraDto(Guid Id, string Nombre, string Codigo);
public record CreateCarreraRequest(string Nombre, string Codigo);
