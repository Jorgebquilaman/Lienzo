namespace Lienzo.Application.DTOs;

public record ClaseResponse(
    Guid Id,
    Guid ReservationId,
    Guid ActividadId,
    Guid ClassroomId,
    string ClassroomName,
    string ActividadNombre,
    DateOnly Fecha,
    TimeOnly HoraInicio,
    TimeOnly HoraFin,
    string Estado,
    DateTime CheckedInAt,
    string? AlumnoNombre,
    List<AsistenciaAlumnoResponse> Alumnos);

public record AsistenciaAlumnoResponse(
    Guid Id,
    int SgaAlumnoId,
    int SgaPersonaId,
    string AlumnoNombre,
    bool Presente,
    Guid? MarcadoPorUsuarioId,
    bool Sincronizado);

public record CheckInResult(
    Guid ClaseId,
    string QrUrl,
    int TotalAlumnos);

public record SyncResult(
    int Actualizados,
    int Errores,
    List<string> Detalle);
