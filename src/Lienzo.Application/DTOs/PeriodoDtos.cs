namespace Lienzo.Application.DTOs;

public record PeriodoDto(Guid Id, string Nombre, string FechaInicio, string FechaFin, int Anio, int? CodigoExterno, Guid? TipoPeriodoId, string? TipoPeriodoNombre);
public record CreatePeriodoRequest(string Nombre, string FechaInicio, string FechaFin, int Anio);
