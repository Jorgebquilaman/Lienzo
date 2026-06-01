namespace Lienzo.Application.DTOs;

public record ActividadDto(
    Guid Id, string Nombre, string CodigoMateria,
    Guid PeriodoId, string? PeriodoNombre,
    Guid CarreraId, string? CarreraNombre,
    Guid? AulaId, string? AulaNombre,
    string? DiaSemana, string? HoraInicio, string? HoraFin,
    List<string> DocenteIds,
    string? DocentesNombres
);

public record CreateActividadRequest(
    string Nombre, string CodigoMateria,
    Guid PeriodoId, Guid CarreraId,
    List<string>? DocenteIds,
    Guid? AulaId, string? DiaSemana, string? HoraInicio, string? HoraFin
);
